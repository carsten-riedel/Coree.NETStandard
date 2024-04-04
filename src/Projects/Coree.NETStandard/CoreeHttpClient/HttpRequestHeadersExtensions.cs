using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Coree.NETStandard.CoreeHttpClient
{
    public static partial class HttpRequestHeadersExtensions
    {
        public static void AddRequestHeaderCookies(this HttpRequestHeaders headers, Dictionary<string, string>? cookies)
        {
            if (cookies == null || !cookies.Any())
            {
                // No cookies to add, so simply return.
                return;
            }

            // Construct the Cookie header value from the dictionary.
            var cookieHeaderValue = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));

            // Add the constructed cookie header to the request.
            headers.Add("Cookie", cookieHeaderValue);
        }
    }
}
