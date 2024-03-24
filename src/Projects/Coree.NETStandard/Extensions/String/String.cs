using System;
using System.Text.RegularExpressions;
using System.Linq;

namespace Coree.NETStandard.Extensions.Strings
{
    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class StringExtensions
    {
        /// <summary>
        /// Splits the input string by a specified string delimiter, with options to control the split operation.
        /// </summary>
        /// <param name="input">The string to be split.</param>
        /// <param name="delimiter">The string delimiter to split by.</param>
        /// <param name="options">Specifies whether to omit empty or whitespace-only substrings from the resulting array.</param>
        /// <returns>An array of substrings that are delimited by the specified string.</returns>
        public static string[] SplitWith(this string input, string delimiter, StringSplitOptions options = StringSplitOptions.None)
        {
            string escapedDelimiter = Regex.Escape(delimiter);
            string[] result = Regex.Split(input, escapedDelimiter);
            return options == StringSplitOptions.RemoveEmptyEntries ? result.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() : result;
        }

        /// <summary>
        /// Splits the input string by an array of characters, treating them as a single connected delimiter, with options to control the split operation.
        /// </summary>
        /// <param name="input">The string to be split.</param>
        /// <param name="delimiter">The array of characters to form the delimiter.</param>
        /// <param name="options">Specifies whether to omit empty or whitespace-only substrings from the resulting array.</param>
        /// <returns>An array of substrings that are delimited by the specified sequence of characters.</returns>
        public static string[] SplitWith(this string input, char[] delimiter, StringSplitOptions options = StringSplitOptions.None)
        {
            var joined = string.Join("", delimiter);
            string escapedDelimiter = Regex.Escape(joined);
            string[] result = Regex.Split(input, escapedDelimiter);
            return options == StringSplitOptions.RemoveEmptyEntries ? result.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() : result;
        }

        /// <summary>
        /// Splits the input string by multiple string delimiters, with options to control the split operation.
        /// </summary>
        /// <param name="input">The string to be split.</param>
        /// <param name="delimiters">The array of string delimiters to split by.</param>
        /// <param name="options">Specifies whether to omit empty or whitespace-only substrings from the resulting array.</param>
        /// <returns>An array of substrings that are delimited by any of the specified strings.</returns>
        public static string[] SplitWith(this string input, string[] delimiters, StringSplitOptions options = StringSplitOptions.None)
        {
            string pattern = "(" + string.Join("|", delimiters.Select(Regex.Escape)) + ")";
            string[] result = Regex.Split(input, pattern);
            return options == StringSplitOptions.RemoveEmptyEntries ? result.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() : result;
        }
    }

}
