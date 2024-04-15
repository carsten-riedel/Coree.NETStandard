using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text.Json;
using System.Xml;
using System.Text;

namespace Coree.NETStandard.Extensions
{
    /// <summary>
    /// Specifies the types of character encodings.
    /// </summary>
    public enum Encodings
    {
        /// <summary>
        /// The default encoding.
        /// </summary>
        Default,

        /// <summary>
        /// ASCII encoding.
        /// </summary>
        ASCII,

        /// <summary>
        /// UTF-7 encoding.
        /// </summary>
        UTF7,

        /// <summary>
        /// UTF-8 encoding.
        /// </summary>
        UTF8,

        /// <summary>
        /// UTF-32 encoding.
        /// </summary>
        UTF32,

        /// <summary>
        /// Unicode encoding.
        /// </summary>
        Unicode,

        /// <summary>
        /// Big Endian Unicode encoding.
        /// </summary>
        BigEndianUnicode,
    }

    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class StringsExtensions
    {
        /// <summary>
        /// Converts a string to a selected text encoding byte array.
        /// </summary>
        /// <param name="String"></param>
        /// <param name="encoding">The encoding type System.Text.Encoding</param>
        /// <returns></returns>
        public static byte[] ToByteArray(this string String, Encodings encoding = Encodings.UTF8)
        {
            Encoding? enc = null;
            switch (encoding)
            {
                case Encodings.Default:
                    enc = Encoding.Default;
                    break;
                case Encodings.ASCII:
                    enc = Encoding.ASCII;
                    break;
                case Encodings.UTF7:
                    enc = Encoding.UTF7;
                    break;
                case Encodings.UTF8:
                    enc = Encoding.UTF8;
                    break;
                case Encodings.UTF32:
                    enc = Encoding.UTF32;
                    break;
                case Encodings.Unicode:
                    enc = Encoding.Unicode;
                    break;
                case Encodings.BigEndianUnicode:
                    enc = Encoding.BigEndianUnicode;
                    break;
                default:
                    break;
            }

            if (enc == null)
            {
                throw new ArgumentNullException(nameof(enc));  
            }

            return enc.GetBytes(String);
        }

        /// <summary>
        /// Converts a string to a byte array using a specified encoding.
        /// </summary>
        /// <param name="String">The string to be converted.</param>
        /// <param name="CodePage">The code page used for encoding.</param>
        /// <returns>A byte array resulting from encoding the string with the specified code page.</returns>
        /// <exception cref="ArgumentException">Thrown when an invalid code page is provided or the string is null.</exception>
        public static byte[] ToByteArray(this string String, int CodePage)
        {
            if (String == null)
            {
                throw new ArgumentException("String cannot be null.", nameof(String));
            }

            Encoding enc;
            try
            {
                enc = Encoding.GetEncoding(CodePage);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to retrieve encoding for the provided code page.", nameof(CodePage), ex);
            }

            return enc.GetBytes(String);
        }
    }
}
