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

using Polly;

namespace Coree.NETStandard.Classes.HttpRequestService
{
    public class HttpRequestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpRequestService> logger;

        public HttpRequestService(ILogger<HttpRequestService> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private HttpRequestMessage ComposeRequestMessage(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, bool ensureUserAgent = true)
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
            return request;
        }

        public string ComposeRequestKeyAsync(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, bool ensureUserAgent = true)
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


        public async Task<TransactionRecord2> Request4(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
        {
            TimeSpan effectiveCacheDuration = cacheDuration ?? TimeSpan.FromSeconds(1);

            string key = ComposeRequestKeyAsync(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);
            if (effectiveCacheDuration > TimeSpan.Zero && _cache.TryGetValue(key, out TransactionRecord2? cachedResult))
            {
                cachedResult!.IsFromCache = true; // Mark as from cache
                return cachedResult;
            }

            // Define the conditions for retry based on HTTP response codes.
            Func<HttpResponseMessage, bool> httpResponseCondition = response =>
                response.StatusCode == HttpStatusCode.RequestTimeout ||
                response.StatusCode == HttpStatusCode.InternalServerError ||
                response.StatusCode == HttpStatusCode.BadGateway ||
                response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.GatewayTimeout;

            var exceptionPolicy = Policy.Handle<Exception>(ex => !(ex is OperationCanceledException));
            var combined = exceptionPolicy.OrResult(httpResponseCondition);

            var retryPolicy = combined.WaitAndRetryAsync(maxTries, retryAttempt =>
                    retryDelay ?? TimeSpan.FromSeconds(3),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Check if the outcome was an exception or a response message and log appropriately
                        if (outcome.Exception != null)
                        {
                            Console.WriteLine($"Retry {retryCount} due to {outcome.Exception.Message}, retrying in {timespan.TotalSeconds}s.");
                        }
                        else
                        {
                            Console.WriteLine($"Retry {retryCount} due to HTTP {outcome.Result.StatusCode}, retrying in {timespan.TotalSeconds}s.");
                        }
                    });

            var fallbackPolicy = Policy<HttpResponseMessage>.Handle<Exception>().FallbackAsync(
        new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.ServiceUnavailable },
                    onFallbackAsync: async b =>
                    {
                        // This is optional, but you can log the failure or handle additional cleanup here.
                        Console.WriteLine("Falling back to an empty HttpResponseMessage.");
                        await Task.CompletedTask;
                    } );

            var policyWrap = Policy.WrapAsync(retryPolicy, fallbackPolicy);

            var client = _httpClientFactory.CreateClient(nameof(HttpRequestService));
            HttpRequestMessage? request = null;
            HttpResponseMessage? response = await policyWrap.ExecuteAsync(async () =>
            {
                try
                {
                    request = ComposeRequestMessage(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);
                    return await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Handle timeouts separately if needed
                    throw new HttpRequestException("Request timed out and will be retried.");
                }
            });

            // Process the response
            var responseResult = new TransactionRecord2(request, response);

            if (effectiveCacheDuration > TimeSpan.Zero)
            {
                _cache.Set(key, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = effectiveCacheDuration });
            }

            return responseResult;
        }
    }
}