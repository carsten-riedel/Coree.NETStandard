using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Coree.NETStandard.Extensions.Conversions.String
{

    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class ConversionsStringExtensions
    {
        /// <summary>
        /// Generates a UUID for the given string using MD5 hashing and returns it as a string without hyphens.
        /// </summary>
        /// <param name="input">The input string for which to generate the UUID. If null, an empty string is used.</param>
        /// <returns>A string representation of the UUID generated from the input string, without hyphens.</returns>
        /// <example>
        /// <code>
        /// string uuid = "your-long-string-here".ToShortUUID();
        /// </code>
        /// </example>
        public static string ToShortUUID(this string? input)
        {
            if (input == null)
            {
                input = string.Empty;
            }

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                Guid guid = new Guid(hash);
                return guid.ToString("N"); // "N" format specifier returns a string without hyphens
            }
        }
    }
}

