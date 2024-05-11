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
    /// <summary>
    /// Manages HTTP requests with advanced features like caching, retries, and request composition.
    /// </summary>
    public class HttpRequestService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpRequestService> _logger;

        // Define the HTTP response condition as a property
        private Func<HttpResponseMessage?, bool> httpResponseCondition => response =>
            response?.StatusCode == HttpStatusCode.RequestTimeout ||
            response?.StatusCode == HttpStatusCode.InternalServerError ||
            response?.StatusCode == HttpStatusCode.BadGateway ||
            response?.StatusCode == HttpStatusCode.ServiceUnavailable ||
            response?.StatusCode == HttpStatusCode.NotFound ||
            response?.StatusCode == HttpStatusCode.GatewayTimeout;

        /// <summary>
        /// Initializes a new instance of the HttpRequestService with specified logging, client factory, and cache.
        /// </summary>
        /// <param name="logger">Logger for logging service operations.</param>
        /// <param name="httpClientFactory">Factory to create HttpClient instances.</param>
        /// <param name="memoryCache">Cache to store request results.</param>
        public HttpRequestService(ILogger<HttpRequestService> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates and executes an HTTP request based on specified parameters, handling retries and caching results.
        /// </summary>
        /// <param name="httpMethod">The HTTP method to be used in the request.</param>
        /// <param name="url">The URL of the request.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <param name="queryParams">Optional query parameters to append to the URL.</param>
        /// <param name="cookies">Optional cookies to include in the request headers.</param>
        /// <param name="httpContentBuilder">Optional builder to create the HTTP content of the request.</param>
        /// <param name="cacheDuration">The duration for which the response should be cached. If null, defaults to 2 seconds.</param>
        /// <param name="retryCount">The number of times the request should retry on failure. Defaults to 3 retries.</param>
        /// <param name="retryDelay">The delay between retries. If null, defaults to 10 seconds.</param>
        /// <param name="requestTimeout">The timeout for the request. If null, defaults to 5 seconds.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation, with a result of the transaction record of the request.</returns>
        /// <remarks>
        /// This method handles retries and caching of results. It uses Polly library for resilience and transient-fault-handling policies.
        /// </remarks>
        public async Task<TransactionRecord> CreateRequest(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, TimeSpan? cacheDuration = null, int retryCount = 3, TimeSpan? retryDelay = null, TimeSpan? requestTimeout = null, CancellationToken cancellationToken = default)
        {
            cacheDuration ??= TimeSpan.FromSeconds(2);
            retryDelay ??= TimeSpan.FromSeconds(10);
            requestTimeout ??= TimeSpan.FromSeconds(5);
            
            var requestTimeoutCts = new CancellationTokenSource(requestTimeout.Value);
            var requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,requestTimeoutCts.Token);
            

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
                else if (outcome.Result != null)
                {
                    _logger.LogInformation($"Retry {retryCount} due to HTTP {outcome.Result.StatusCode}, retrying in {timespan.TotalSeconds}s.");
                } 
                else
                {
                    _logger.LogInformation($"Retry {retryCount} retrying in {timespan.TotalSeconds}s.");
                }
                await Task.CompletedTask;
            });


            var client = _httpClientFactory.CreateClient(nameof(HttpRequestService));
            HttpRequestMessage? request = null;
            HttpResponseMessage? response = null;
            Exception? exception = null;
            try
            {
                response = await retryPolicy.ExecuteAsync(async () =>
                {
                    try
                    {
                        request = ComposeRequestMessage(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);
                        requestTimeoutCts = new CancellationTokenSource(requestTimeout.Value);
                        requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, requestTimeoutCts.Token);
                        return await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, requestCts.Token);
                    }
                    catch (OperationCanceledException ex)
                    {
                        if (requestTimeoutCts.IsCancellationRequested)
                        {
                            throw new HttpRequestException("Request timed out.", ex);
                        }
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Operation was canceled by the user.");
                            exception = new OperationCanceledException("Operation was canceled by the user");
                            return null;
                        }
                        else
                        {
                            throw new HttpRequestException("Unkown OperationCanceled request", ex);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Operation failed after retries on {Url} with exception: {ExceptionMessage}", url, ex.Message);
                exception = ex;
            }

            var responseResult = new TransactionRecord(request, response,exception);

            if (cacheDuration > TimeSpan.Zero)
            {
                _cache.Set(key, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheDuration });
            }

            return responseResult;
        }

        /// <summary>
        /// Composes an HTTP request message using specified parameters.
        /// </summary>
        /// <param name="httpMethod">The HTTP method for the request.</param>
        /// <param name="url">The fully qualified URL for the request.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <param name="queryParams">Optional query parameters to append to the URL.</param>
        /// <param name="cookies">Optional cookies to include in the request headers.</param>
        /// <param name="httpContentBuilder">Optional builder to create the HTTP content of the request.</param>
        /// <param name="ensureUserAgent">Ensures that a User-Agent header is included. Defaults to true.</param>
        /// <param name="ensureCorrelationID">Ensures that a Correlation-ID header is included. Defaults to true.</param>
        /// <returns>The composed HttpRequestMessage.</returns>
        /// <remarks>
        /// This method ensures that all necessary headers, including User-Agent and Correlation-ID, are included in the request.
        /// </remarks>
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

        /// <summary>
        /// Composes a unique key for caching the response of an HTTP request.
        /// </summary>
        /// <param name="httpMethod">The HTTP method for the request.</param>
        /// <param name="url">The fully qualified URL for the request.</param>
        /// <param name="headers">Optional headers to include in the request.</param>
        /// <param name="queryParams">Optional query parameters to append to the URL.</param>
        /// <param name="cookies">Optional cookies to include in the request headers.</param>
        /// <param name="httpContentBuilder">Optional builder to create the HTTP content of the request.</param>
        /// <param name="ensureUserAgent">Ensures that a User-Agent header is included. Defaults to true.</param>
        /// <returns>A string representing the unique key for caching.</returns>
        /// <remarks>
        /// This key is used to uniquely identify and cache the results of HTTP requests to improve performance and reduce redundancy.
        /// </remarks>
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
    }


}