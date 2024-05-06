using System;
using System.Net.Http.Headers;
using System.Text;

using System.Text.RegularExpressions;

namespace Coree.NETStandard.Extensions.Http.HttpHeader
{
    public static partial class HttpHeadersExtensions
    {
        public static Encoding GetContentEncoding(this HttpHeaders headers)
        {
            // Default to UTF-8 if the content encoding is not specified or unrecognized
            var encoding = Encoding.UTF8;

            // Attempt to read the charset from the Content-Type header
            if (headers.TryGetValues("Content-Type", out var values))
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
                        // This catch block can also be used to log the error if necessary
                    }
                }
            }

            return encoding;
        }
    }
}
