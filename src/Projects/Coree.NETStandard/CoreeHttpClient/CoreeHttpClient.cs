using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using Json.More;
using Json.Path;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.CoreeHttpClient
{
    //public interface ICoreeHttpClient
    //{
    //    Task<HttpResponseResult> GetAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default);

    //    Task<string?> GetStringAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default);

    //    Task<JsonDocument?> GetJsonDocumentAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default);

    //    Task<JsonNode?> GetJsonNodeAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default);

    //    Task<PathResult?> GetJsonPathResultAsync(string url, string jsonPath, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default);

    //    Task<List<T>?> GetJsonPathResultAsync<T>(string url, string jsonPath, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default);
    //}

    //public class CoreeHttpClient : ICoreeHttpClient
    //{
    //    private readonly IHttpClientFactory _httpClientFactory;
    //    private readonly IMemoryCache _cache;
    //    private readonly ILogger<CoreeHttpClient> logger;

    //    public CoreeHttpClient(ILogger<CoreeHttpClient> logger, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    //    {
    //        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    //        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    //        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    //    }

    //    public async Task<HttpResponseResult> GetAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    //    {
    //        TimeSpan effectiveCacheDuration = cacheDuration ?? TimeSpan.FromSeconds(5);
    //        retryDelay ??= TimeSpan.FromSeconds(5);
    //        Exception? lastException = null;

    //        // Attempt to retrieve the cached HttpResponseResult
    //        if (effectiveCacheDuration > TimeSpan.Zero && _cache.TryGetValue(url, out HttpResponseResult? cachedResult))
    //        {
    //            cachedResult!.IsFromCache = true; // Mark as from cache
    //            return cachedResult;
    //        }

    //        for (int tryCount = 0; tryCount < maxTries; tryCount++)
    //        {
    //            try
    //            {
    //                var client = _httpClientFactory.CreateClient("CoreeHttpClient");
    //                var request = new HttpRequestMessage(HttpMethod.Get, url);

    //                // Add headers to the request if any
    //                headers?.ToList().ForEach(header => request.Headers.TryAddWithoutValidation(header.Key, header.Value));

    //                // Pass cancellationToken to SendAsync
    //                var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

    //                if (!response.IsSuccessStatusCode)
    //                {
    //                    // Throwing an exception for non-success status codes
    //                    throw new HttpRequestException($"Request to {url} failed with status code {response.StatusCode}, reason: {response.ReasonPhrase}");
    //                }

    //                // Pass cancellationToken to ReadAsByteArrayAsync
    //                var contentBytes = await response.Content.ReadAsByteArrayAsync();
    //                var responseResult = new HttpResponseResult(contentBytes, response.Headers, request.Headers, false, response.StatusCode);

    //                if (effectiveCacheDuration > TimeSpan.Zero)
    //                {
    //                    _cache.Set(url, responseResult, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = effectiveCacheDuration });
    //                }

    //                return responseResult;
    //            }
    //            catch (HttpRequestException ex)
    //            {
    //                lastException = ex;
    //                if (tryCount < maxTries - 1) await Task.Delay(retryDelay.Value, cancellationToken); // Also pass cancellationToken to Task.Delay
    //            }
    //            catch (OperationCanceledException)
    //            {
    //                // If cancellation was requested, propagate the cancellation exception immediately without retrying.
    //                throw;
    //            }
    //        }

    //        // Return failure result with the last exception
    //        return new HttpResponseResult(lastException, HttpStatusCode.ServiceUnavailable);
    //    }

    //    public async Task<string?> GetStringAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    //    {
    //        var httpResponseResult = await GetAsync(url, headers, cacheDuration, maxTries, retryDelay, cancellationToken);
    //        if (httpResponseResult.Status == HttpResponseResult.OperationStatus.Failure) { return null; }
    //        return httpResponseResult.ContentString;
    //    }

    //    public async Task<JsonDocument?> GetJsonDocumentAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    //    {
    //        var jsonString = await GetStringAsync(url, headers, cacheDuration, maxTries, retryDelay, cancellationToken);
    //        if (jsonString == null) { return null; }
    //        return JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
    //    }

    //    public async Task<JsonNode?> GetJsonNodeAsync(string url, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    //    {
    //        var jsonString = await GetStringAsync(url, headers, cacheDuration, maxTries, retryDelay, cancellationToken);
    //        if (jsonString == null) { return null; }
    //        var doc = JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
    //        return doc.RootElement.AsNode();
    //    }

    //    public async Task<PathResult?> GetJsonPathResultAsync(string url, string jsonPath, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    //    {
    //        var jsonString = await GetStringAsync(url, headers, cacheDuration, maxTries, retryDelay, cancellationToken);
    //        if (jsonString == null) { return null; }
    //        var path = JsonPath.Parse(jsonPath, new PathParsingOptions() { });
    //        var doc = JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
    //        var result = path.Evaluate(doc.RootElement.AsNode());
    //        return result;
    //    }

    //    public async Task<List<T>?> GetJsonPathResultAsync<T>(string url, string jsonPath, Dictionary<string, string>? headers = null, TimeSpan? cacheDuration = null, int maxTries = 3, TimeSpan? retryDelay = null, CancellationToken cancellationToken = default)
    //    {
    //        var pathResult = await GetJsonPathResultAsync(url, jsonPath, headers, cacheDuration, maxTries, retryDelay, cancellationToken);
    //        if (pathResult == null) { return null; } // Or new List<T>() if you prefer not to return null
    //        var result = pathResult.Matches.Select(e => e.Value.GetValue<T>()).ToList();
    //        return result;
    //    }
    //}
}