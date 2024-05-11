using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace Coree.NETStandard.Extensions.Http.HttpHeader
{

    /// <summary>
    /// Provides extension methods for handling and parsing HTTP headers.
    /// </summary>
    public static partial class HttpHeadersExtensions
    {
        /// <summary>
        /// Retrieves the character encoding from the Content-Type HTTP header.
        /// </summary>
        /// <param name="headers">The collection of HTTP headers.</param>
        /// <param name="defaultEncoding">The default encoding to use if no encoding is specified or if the specified encoding is unrecognized. Defaults to UTF-8.</param>
        /// <returns>The detected Encoding or the default if not specified.</returns>
        public static Encoding GetContentEncoding(this HttpHeaders headers, Encoding? defaultEncoding = null)
        {
            defaultEncoding ??= Encoding.UTF8;  // Use UTF-8 as the default encoding if none is provided.

            // Attempt to read the charset from the Content-Type header
            if (headers.TryGetValues("Content-Type", out var values))
            {
                foreach (var value in values)
                {
                    var match = Regex.Match(value, @"charset\s*=\s*['""]?([\w-]+)['""]?", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var charset = match.Groups[1].Value;
                        try
                        {
                            return Encoding.GetEncoding(charset);
                        }
                        catch (ArgumentException)
                        {
                            // If the encoding is not supported, fall back to UTF-8
                            Console.WriteLine($"Unsupported encoding specified: {charset}. Defaulting to UTF-8.");
                        }
                    }
                }
            }

            return defaultEncoding;
        }
    }

}
