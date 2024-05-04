using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.CoreeHttpClient;

using Microsoft.Extensions.Caching.Memory;

using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Http.Headers;
using Polly.Retry;
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

        private HttpRequestMessage ComposeRequest(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, bool ensureUserAgent = true)
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

        public async Task<TransactionRecord2> Request3(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
        {
            TimeSpan effectiveCacheDuration = cacheDuration ?? TimeSpan.FromSeconds(5);

            // Attempt to retrieve the cached HttpResponseResult
            if (effectiveCacheDuration > TimeSpan.Zero && _cache.TryGetValue(url, out TransactionRecord2? cachedResult))
            {
                cachedResult!.IsFromCache = true; // Mark as from cache
                return cachedResult;
            }

            var client = _httpClientFactory.CreateClient(nameof(HttpRequestService));

            var request = ComposeRequest(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);


            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

            var responseResult = new TransactionRecord2(request, response);


            if (effectiveCacheDuration > TimeSpan.Zero)
            {
                _cache.Set(url, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = effectiveCacheDuration });
            }

            return responseResult;
        }

        public async Task<TransactionRecord2> Request4(HttpMethod httpMethod, Uri url, Dictionary<string, string>? headers = null, Dictionary<string, string>? queryParams = null, Dictionary<string, string>? cookies = null, ContentComposer? httpContentBuilder = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
        {

            TimeSpan effectiveCacheDuration = cacheDuration ?? TimeSpan.FromSeconds(5);

            // Attempt to retrieve the cached HttpResponseResult
            if (effectiveCacheDuration > TimeSpan.Zero && _cache.TryGetValue(url, out TransactionRecord2? cachedResult))
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

            // Create the policy for exceptions.
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

            var client = _httpClientFactory.CreateClient(nameof(HttpRequestService));
            HttpRequestMessage? request = null;
            HttpResponseMessage response = await retryPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    request = ComposeRequest(httpMethod, url, headers, queryParams, cookies, httpContentBuilder);
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

            if (cacheDuration.HasValue && cacheDuration > TimeSpan.Zero)
            {
                string key = request.GetType().Name + request.GetHashCode();
                _cache.Set(url, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = cacheDuration.Value });
            }

            return responseResult;
        }
    }
}
