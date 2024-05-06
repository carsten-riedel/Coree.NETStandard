using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Coree.NETStandard.Classes.RateLimiter;

using Polly;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    public class HttpRequestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpRequestService> _logger;
        private readonly RateLimit<TransactionRecord> _rateLimiter;

        // Define the HTTP response condition as a property
        private Func<HttpResponseMessage, bool> httpResponseCondition => response =>
            response.StatusCode == HttpStatusCode.RequestTimeout ||
            response.StatusCode == HttpStatusCode.InternalServerError ||
            response.StatusCode == HttpStatusCode.BadGateway ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.GatewayTimeout;

        public HttpRequestService(ILogger<HttpRequestService> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _rateLimiter = new RateLimit<TransactionRecord>(1, TimeSpan.FromSeconds(15));
        }

        public async Task<TransactionRecord> CreateRequest(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, TimeSpan? cacheDuration = null, int retryCount = 3, TimeSpan? retryDelay = null, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
        {
            cacheDuration ??= TimeSpan.FromSeconds(2);
            retryDelay ??= TimeSpan.FromSeconds(3);
            requestTimeout ??= TimeSpan.FromSeconds(10);

            var timeoutCts = new CancellationTokenSource(requestTimeout.Value);
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            string key = ComposeRequestKey(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);
            if (cacheDuration > TimeSpan.Zero && _cache.TryGetValue(key, out TransactionRecord? cachedResult))
            {
                cachedResult!.IsFromCache = true; // Mark as from cache
                return cachedResult;
            }

            var exceptionConditionPolicy = Policy.Handle<Exception>(ex => ex is not OperationCanceledException).OrResult(httpResponseCondition);

            var retryPolicy = exceptionConditionPolicy.WaitAndRetryAsync(retryCount, _ => retryDelay.Value, async (outcome, timespan, retryCount, context) =>
            {
                if (outcome.Exception != null)
                {
                    _logger.LogInformation($"Retry {retryCount} due to {outcome.Exception.Message}, retrying in {timespan.TotalSeconds}s.");
                }
                else
                {
                    _logger.LogInformation($"Retry {retryCount} due to HTTP {outcome.Result.StatusCode}, retrying in {timespan.TotalSeconds}s.");
                }
                await Task.CompletedTask;
            });


            var client = _httpClientFactory.CreateClient(nameof(HttpRequestService));
            HttpRequestMessage? request = null;
            HttpResponseMessage? response = null;
            try
            {
                response = await retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        request = ComposeRequestMessage(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);
                        return await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, linkedCts.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (linkedCts.IsCancellationRequested)
                        {
                            _logger.LogInformation("Operation was canceled.");
                            return new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable };
                        }
                        else
                        {
                            throw new HttpRequestException("Request timed out and will be retried.", ex);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Operation failed after retries on {Url} with exception: {ExceptionMessage}", url, ex.Message);
                response = null;
            }

            var responseResult = new TransactionRecord(request, response);

            if (cacheDuration > TimeSpan.Zero)
            {
                _cache.Set(key, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheDuration });
            }

            return responseResult;
        }

        public async Task<TransactionRecord> PerformTask(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, TimeSpan? cacheDuration = null, int retryCount = 3, TimeSpan? retryDelay = null, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
        {
            return await _rateLimiter.EnqueueTask(() => CreateRequest(httpMethod, url, headers, queryParams, cookies, httpContentBuilder, cacheDuration, retryCount, retryDelay, requestTimeout, cancellationToken));
        }


        private HttpRequestMessage ComposeRequestMessage(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, bool ensureUserAgent = true, bool ensureCorrelationID = true)
        {
            url = url.AddOrUpdateQueryParameters(queryParams);
            var request = new HttpRequestMessage(httpMethod, url);
            request.Headers.AddRequestHeaderCookies(cookies);
            headers?.ToList().ForEach(header => request.Headers.Add(header.Key, header.Value));
            request.Content = httpContentBuilder?.Build();

            if (ensureUserAgent)
            {
                if (!request.Headers.Contains("User-Agent"))
                {
                    request.Headers.Add("User-Agent", "curl/8.7.0");
                }
            }

            if (ensureCorrelationID)
            {
                if (!request.Headers.Contains("X-Correlation-ID"))
                {
                    request.Headers.Add("X-Correlation-ID", Guid.NewGuid().ToString());
                }
            }

            return request;
        }

        private string ComposeRequestKey(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, bool ensureUserAgent = true)
        {
            var seperator = Environment.NewLine;
            using (var sha256 = SHA256.Create())
            {
                var hashParts = new List<byte[]>
            {
                Encoding.Unicode.GetBytes(httpMethod.Method),
                Encoding.Unicode.GetBytes(url.AbsoluteUri)
            };

                if (queryParams != null)
                {
                    var queryString = String.Join(seperator, queryParams.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    hashParts.Add(Encoding.Unicode.GetBytes(queryString));
                }

                if (headers != null)
                {
                    var headerString = String.Join(seperator, headers.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}:{kvp.Value}"));
                    hashParts.Add(Encoding.Unicode.GetBytes(headerString));
                }

                if (cookies != null)
                {
                    var cookieString = String.Join(seperator, cookies.OrderBy(kvp => kvp.Key).Select(kvp => $"{kvp.Key}={kvp.Value}"));
                    hashParts.Add(Encoding.Unicode.GetBytes(cookieString));
                }

                if (httpContentBuilder != null)
                {
                    var contentBytes = httpContentBuilder.Build()?.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    hashParts.Add(sha256.ComputeHash(contentBytes));
                }

                if (ensureUserAgent && (headers == null || !headers.ContainsKey("User-Agent")))
                {
                    hashParts.Add(Encoding.Unicode.GetBytes("User-Agent:curl/8.7.0"));
                }

                var combinedHash = sha256.ComputeHash(hashParts.SelectMany(x => x).ToArray());
                return BitConverter.ToString(combinedHash).Replace("-", "").ToLowerInvariant();
            }
        }

        public async Task InvokeDump()
        {
            Console.WriteLine("dump");
            await Task.CompletedTask;
        }
    }
}