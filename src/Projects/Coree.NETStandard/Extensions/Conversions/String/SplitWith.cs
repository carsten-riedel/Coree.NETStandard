using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Coree.NETStandard.Extensions.Conversions.String
{
    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class ConversionsStringExtensions
    {
        /// <summary>
        /// Splits the input string by a specified string delimiter, with options to control the split operation. Offers the option to remove the delimiter from the resulting array.
        /// </summary>
        /// <param name="input">The string to be split.</param>
        /// <param name="delimiter">The string delimiter to split by.</param>
        /// <param name="options">Specifies whether to omit empty or whitespace-only substrings from the resulting array.</param>
        /// <param name="removeDelimiters">When set to true, the delimiter itself is excluded from the result array.</param>
        /// <returns>An array of substrings that are delimited by the specified string, optionally excluding the delimiter itself.</returns>
        /// <remarks>
        /// This method uses regular expressions to split the input string, which may have performance implications for large strings or a large number of delimiters.
        /// When removeDelimiters is true, it filters out the delimiter if it matches exactly, after splitting.
        /// </remarks>
        public static string[] SplitWith(this string? input, string delimiter, StringSplitOptions options = StringSplitOptions.None, bool removeDelimiters = true)
        {
            return input.SplitWith(new string[] { delimiter }, options, removeDelimiters);
        }

        /// <summary>
        /// Splits the input string by an array of characters, treating them as individual delimiters, with options to control the split operation. Offers the option to remove the delimiters from the resulting array.
        /// </summary>
        /// <param name="input">The string to be split.</param>
        /// <param name="delimiter">The array of characters joined to a string to use as delimiter</param>
        /// <param name="options">Specifies whether to omit empty or whitespace-only substrings from the resulting array.</param>
        /// <param name="removeDelimiters">When set to true, the delimiters themselves are excluded from the result array.</param>
        /// <returns>An array of substrings that are delimited by the specified characters, optionally excluding the delimiters themselves.</returns>
        /// <remarks>
        /// This method constructs a string from the character array and uses regular expressions for splitting, which may have performance implications for large strings or a large number of delimiters.
        /// When removeDelimiters is true, it filters out any substring that matches one of the delimiters exactly, after splitting.
        /// </remarks>
        public static string[] SplitWith(this string? input, char[] delimiter, StringSplitOptions options = StringSplitOptions.None, bool removeDelimiters = true)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<string>();

            var joined = string.Join("", delimiter);
            return input.SplitWith(new string[] { joined }, options, removeDelimiters);
        }

        /// <summary>
        /// Splits the input string by multiple string delimiters, with options to control the split operation. Offers the option to remove the delimiters from the resulting array.
        /// </summary>
        /// <param name="input">The string to be split.</param>
        /// <param name="delimiters">The array of string delimiters to split by.</param>
        /// <param name="options">Specifies whether to omit empty or whitespace-only substrings from the resulting array.</param>
        /// <param name="removeDelimiters">When set to false, the delimiters themselves are not excluded from the result array.</param>
        /// <returns>An array of substrings that are delimited by any of the specified strings, optionally excluding the delimiters themselves.</returns>
        /// <remarks>
        /// This method uses regular expressions to split the input string, which may have performance implications for large strings or a large number of delimiters.
        /// When removeDelimiters is true, it filters out any substring that matches one of the delimiters exactly, after splitting.
        /// </remarks>
        public static string[] SplitWith(this string? input, string[] delimiters, StringSplitOptions options = StringSplitOptions.None, bool removeDelimiters = true)
        {
            if (string.IsNullOrEmpty(input))
                return Array.Empty<string>();

            // Joining the delimiters into a regex pattern.
            string pattern = "(" + string.Join("|", delimiters.Select(Regex.Escape)) + ")";
            string[] result = Regex.Split(input, pattern);

            if (removeDelimiters)
            {
                // Filter out the delimiters from the results using LINQ, checking for any match.
                result = result.Where(part => !delimiters.Any(delimiter => part == delimiter)).ToArray();
            }

            // Applying StringSplitOptions as before.
            if (options == StringSplitOptions.RemoveEmptyEntries)
            {
                result = result.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
            }

            return result;
        }
    }
}