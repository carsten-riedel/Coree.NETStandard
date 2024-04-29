using System;

namespace Coree.NETStandard.Extensions.Conversions.ByteArray
{
    /// <summary>
    /// Provides extension methods for working with byte arrays values.
    /// </summary>
    public static partial class ConversionsByteArrayExtensions
    {
        /// <summary>
        /// Converts a byte array to a Base64 encoded string.
        /// </summary>
        /// <param name="bytes">The byte array to encode.</param>
        /// <returns>A Base64 encoded string.</returns>
        public static string ToBase64String(this byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}