using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Coree.NETStandard.CoreeHttpClient
{
    /// <summary>
    /// Extends the <see cref="HttpRequestHeaders"/> class with additional methods for convenient header manipulation.
    /// </summary>
    /// <remarks>
    /// This class provides extension methods for the <see cref="HttpRequestHeaders"/> class, allowing for enhanced and simplified operations such as adding or updating cookies and other header values. These extensions are designed to improve the readability and maintainability of code that interacts with HTTP request headers.
    /// </remarks>
    public static partial class HttpRequestHeadersExtensions
    {
        /// <summary>
        /// Adds or updates the Cookie header in the HttpRequestHeaders.
        /// </summary>
        /// <param name="headers">The HttpRequestHeaders to which the Cookie header will be added or updated.</param>
        /// <param name="cookies">A dictionary containing the cookies to be added to the header.</param>
        /// <param name="forceUpdate">If true, existing cookie values will be replaced. If false, an exception will be thrown if the header already contains cookies.</param>
        /// <exception cref="ArgumentException">Thrown when forceUpdate is false and the Cookie header already exists.</exception>
        /// <remarks>
        /// This method allows for flexible management of the Cookie header within HttpRequestHeaders, providing the option to either enforce the addition of new cookies by replacing any existing ones or to prevent modification if cookies are already present.
        /// </remarks>
        public static void AddRequestHeaderCookies(this HttpRequestHeaders headers, Dictionary<string, string>? cookies, bool forceUpdate = true)
        {
            if (cookies == null || !cookies.Any())
            {
                return;
            }

            var cookieHeaderValue = string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));

            if (headers.Contains("Cookie"))
            {
                if (forceUpdate)
                {
                    // Remove existing Cookie header before adding the new value.
                    headers.Remove("Cookie");
                }
                else
                {
                    // Throw an exception as the header already contains cookies and forceUpdate is false.
                    throw new ArgumentException("The HttpRequestHeaders already contains a Cookie header. Set forceUpdate to true to replace existing cookies.");
                }
            }

            headers.Add("Cookie", cookieHeaderValue);
        }

    }
}
