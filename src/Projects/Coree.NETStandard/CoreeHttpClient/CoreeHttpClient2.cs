using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Json.Path;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.CoreeHttpClient
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreeHttpClient2(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.AddHttpClient("CoreeHttpClient2", (provider, client) =>
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")
                {
                    Parameters = { new NameValueHeaderValue("charset", "utf-8") }
                });
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
                client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            // Register CoreeHttpClient as a singleton implementation of ICoreeHttpClient
            services.AddTransient<CoreeHttpClient2>();

            return services;
        }
    }

    public class HttpResponseResult2
    {
        public enum OperationStatus
        {
            Success,
            Failure
        }

        public byte[]? ContentBytes { get; set; }
        public HttpResponseHeaders? ResponseHeaders { get; set; }
        public HttpRequestHeaders? RequestHeaders { get; set; }
        public bool IsFromCache { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Exception? Exception { get; set; } // Exception information
        public OperationStatus Status { get; set; }
        public long? ContentLength { get; set; }

        public string ContentString
        {
            get
            {
                if (ContentBytes == null || ResponseHeaders == null)
                {
                    return string.Empty;
                }
                else
                {
                    try
                    {
                        return ResponseHeaders.GetContentEncoding().GetString(ContentBytes);
                    }
                    catch
                    {
                        return string.Empty; // In case decoding fails
                    }
                }
            }
        }

        // Constructor for successful response
        public HttpResponseResult2(byte[]? contentBytes, HttpResponseHeaders? responseHeaders, HttpRequestHeaders? requestHeaders, bool isFromCache, HttpStatusCode statusCode)
        {
            ContentBytes = contentBytes;
            ResponseHeaders = responseHeaders;
            RequestHeaders = requestHeaders;
            IsFromCache = isFromCache;
            StatusCode = statusCode;
            Status = OperationStatus.Success;
        }

        // Constructor for failure
        public HttpResponseResult2(Exception? exception, HttpStatusCode statusCode)
        {
            Exception = exception;
            StatusCode = statusCode;
            Status = OperationStatus.Failure;
        }
    }

    public class CoreeHttpClient2
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CoreeHttpClient2> logger;

        public CoreeHttpClient2(ILogger<CoreeHttpClient2> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<HttpResponseResult2> GetAsync(HttpMethod httpMethod, Uri baseUrl, RequestParamBuilder? requestParamBuilder = null, RequestContentBuilder? httpContentBuilder = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
        {
            baseUrl = baseUrl.AddOrUpdateQueryParameters(requestParamBuilder?.QueryParams);

            TimeSpan effectiveCacheDuration = cacheDuration ?? TimeSpan.FromSeconds(5);
            retryDelay ??= TimeSpan.FromSeconds(5);
            Exception? lastException = null;

            // Attempt to retrieve the cached HttpResponseResult
            if (effectiveCacheDuration > TimeSpan.Zero && _cache.TryGetValue(baseUrl, out HttpResponseResult2? cachedResult))
            {
                cachedResult!.IsFromCache = true; // Mark as from cache
                return cachedResult;
            }

            for (int tryCount = 0; tryCount < maxTries; tryCount++)
            {
                try
                {
                    var client = _httpClientFactory.CreateClient("CoreeHttpClient2");

                    var request = new HttpRequestMessage(httpMethod, baseUrl);
                    request.Headers.AddRequestHeaderCookies(requestParamBuilder?.Cookies);
                    request.Content = httpContentBuilder?.Build();

                    requestParamBuilder?.Headers?.ToList().ForEach(header => request.Headers.TryAddWithoutValidation(header.Key, header.Value));

                    // Pass cancellationToken to SendAsync
                    var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        // Throwing an exception for non-success status codes
                        throw new HttpRequestException($"Request to {baseUrl} failed with status code {response.StatusCode}, reason: {response.ReasonPhrase}");
                    }

                    // Pass cancellationToken to ReadAsByteArrayAsync
                    var contentBytes = await response.Content.ReadAsByteArrayAsync();
                    var responseResult = new HttpResponseResult2(contentBytes, response.Headers, request.Headers, false, response.StatusCode)
                    {
                        ContentLength = response.Content.Headers.ContentLength
                    };

                    if (effectiveCacheDuration > TimeSpan.Zero)
                    {
                        _cache.Set(baseUrl, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = effectiveCacheDuration });
                    }

                    return responseResult;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (tryCount < maxTries - 1) await Task.Delay(retryDelay.Value, cancellationToken); // Also pass cancellationToken to Task.Delay
                }
                catch (OperationCanceledException)
                {
                    // If cancellation was requested, propagate the cancellation exception immediately without retrying.
                    throw;
                }
            }

            // Return failure result with the last exception
            return new HttpResponseResult2(lastException, HttpStatusCode.ServiceUnavailable);
        }
    }
}