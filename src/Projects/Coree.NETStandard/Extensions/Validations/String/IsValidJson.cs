using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Coree.NETStandard.Extensions.Validations.String
{
    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class ValidationsStringExtensions
    {
        /// <summary>
        /// Validates whether the specified string is valid JSON.
        /// </summary>
        /// <param name="jsonString">The JSON string to validate.</param>
        /// <returns>true if the string is valid JSON; otherwise, false.</returns>
        public static bool IsValidJson(this string jsonString)
        {
            try
            {
                JsonSerializer.Deserialize<object>(jsonString);
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}
