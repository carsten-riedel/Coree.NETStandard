using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Json.More;
using Json.Path;

using Microsoft.Extensions.Caching.Memory;

using static Coree.NETStandard.CoreeHttpClient.CoreeHttpClient;

namespace Coree.NETStandard.CoreeHttpClient
{

    public interface ICoreeHttpClient
    {
        Task<HttpResponseResult> GetAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null);
        Task<string> GetStringAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null);
        Task<JsonDocument> GetJsonDocumentAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null);
        Task<JsonNode?> GetJsonNodeAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null);
        Task<PathResult?> GetJsonPathResultAsync(string url, string jsonPath, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null);
        Task<List<T>> GetJsonPathResultAsync<T>(string url, string jsonPath, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null);
    }

    public class CoreeHttpClient : ICoreeHttpClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;

        public class HttpResponseResult
        {
            public byte[] ContentBytes { get; set; }
            private HttpResponseHeaders ResponseHeaders { get; set; }
            public HttpRequestHeaders RequestHeaders { get; set; }

            // Dynamically determine the content string using the response's content encoding
            public string ContentString
            {
                get
                {
                    if (ContentBytes == null)
                    {
                        return string.Empty;
                    }

                    // Default to UTF-8 if the content encoding is not specified or unrecognized
                    var encoding = Encoding.UTF8;

                    // Attempt to read the charset from the Content-Type header
                    if (ResponseHeaders.TryGetValues("Content-Type", out var values))
                    {
                        var contentType = string.Join(" ", values);
                        var match = Regex.Match(contentType, @"charset=([\w-]+)", RegexOptions.IgnoreCase);

                        if (match.Success)
                        {
                            try
                            {
                                encoding = Encoding.GetEncoding(match.Groups[1].Value);
                            }
                            catch (ArgumentException)
                            {
                                // If the encoding is not supported, fall back to UTF-8
                            }
                        }
                    }

                    return encoding.GetString(ContentBytes);
                }
            }

            public HttpResponseResult(byte[] contentBytes, HttpResponseHeaders responseHeaders, HttpRequestHeaders requestHeaders)
            {
                ContentBytes = contentBytes;
                ResponseHeaders = responseHeaders;
                RequestHeaders = requestHeaders;
            }
        }


        public CoreeHttpClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        }

        public async Task<HttpResponseResult> GetAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null)
        {
            TimeSpan effectiveCacheDuration = cacheDuration ?? TimeSpan.FromSeconds(5);

            // Attempt to retrieve the cached HttpResponseResult
            if (effectiveCacheDuration > TimeSpan.Zero && _cache.TryGetValue(url, out HttpResponseResult cachedResult))
            {
                return cachedResult;
            }

            var client = _httpClientFactory.CreateClient("CoreeHttpClient");
            var request = new HttpRequestMessage(HttpMethod.Get, url);

            // Add headers to the request if any
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            // Read the content as a byte array
            var contentBytes = await response.Content.ReadAsByteArrayAsync();

            // Create the HttpResponseResult object
            var responseResult = new HttpResponseResult(contentBytes, response.Headers, request.Headers);

            // Cache the HttpResponseResult object
            if (effectiveCacheDuration > TimeSpan.Zero)
            {
                _cache.Set(url, responseResult, new MemoryCacheEntryOptions
                {
                    
                    AbsoluteExpirationRelativeToNow = effectiveCacheDuration
                });
            }

            return responseResult;
        }


        public async Task<string> GetStringAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null)
        {
            var httpResponseResult = await GetAsync(url, headers, cacheDuration);
            return httpResponseResult.ContentString;
        }

        public async Task<JsonDocument> GetJsonDocumentAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null)
        {
            // Use GetStringAsync to fetch the content
            var jsonString = await GetStringAsync(url, headers, cacheDuration);
            // Parse the JSON string into JsonDocument
            return JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
        }

        public async Task<JsonNode?> GetJsonNodeAsync(string url, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null)
        {
            // Use GetStringAsync to fetch the content
            var jsonString = await GetStringAsync(url, headers, cacheDuration);
            // Parse the JSON string into JsonNode
            var doc = JsonDocument.Parse(jsonString, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip });
            return doc.RootElement.AsNode();
        }

        public async Task<PathResult?> GetJsonPathResultAsync(string url,string jsonPath, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null)
        {
            
            var jsonString = await GetJsonNodeAsync(url, headers, cacheDuration);
            var path = JsonPath.Parse(jsonPath, new PathParsingOptions() { });
            var result = path.Evaluate(jsonString);
            return result;
        }

        public async Task<List<T>> GetJsonPathResultAsync<T>(string url, string jsonPath, Dictionary<string, string> headers = null, TimeSpan? cacheDuration = null)
        {
            var pathResult = await GetJsonPathResultAsync(url, jsonPath, headers, cacheDuration);
            var result = pathResult.Matches.Select(e => e.Value.GetValue<T>() ).ToList();
            return result;
        }
    }
}
