using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Extensions.Conversions.String
{

    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class ConversionsStringExtensions
    {
        /// <summary>
        /// Trims a specified number of characters from both the start and the end of the string.
        /// </summary>
        /// <param name="input">The string to trim.</param>
        /// <param name="charToTrim">The character to remove from the string.</param>
        /// <param name="countFromStart">The number of characters to trim from the start, defaults to 1 if not specified.</param>
        /// <param name="countFromEnd">The number of characters to trim from the end, defaults to 1 if not specified.</param>
        /// <returns>The trimmed string, or the original string if it is null or empty.</returns>
        /// <example>
        /// string example = "aaaSampleStringaaa";
        /// string trimmed = example.TrimSpecific('a'); // Equivalent to TrimSpecific('a', 1, 1)
        /// Console.WriteLine(trimmed); // Output: "aaSampleStringaa"
        /// </example>
        public static string TrimSpecific(this string input, char charToTrim, int countFromStart = 1, int countFromEnd = 1)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            int start = 0, end = input.Length;

            // Trim from start
            for (int i = 0; i < countFromStart && start < end && input[start] == charToTrim; i++)
                start++;

            // Trim from end
            for (int i = 0; i < countFromEnd && end > start && input[end - 1] == charToTrim; i++)
                end--;

            return start >= end ? string.Empty : input.Substring(start, end - start);
        }
    }
}

