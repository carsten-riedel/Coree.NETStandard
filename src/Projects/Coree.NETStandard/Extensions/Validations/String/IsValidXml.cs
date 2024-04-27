using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Coree.NETStandard.Extensions.Validations.String
{
    /// <summary>
    /// Provides extension methods for string operations, enhancing the built-in string manipulation capabilities.
    /// </summary>
    public static partial class ValidationsStringExtensions
    {
        /// <summary>
        /// Validates whether the specified string is well-formed XML.
        /// </summary>
        /// <param name="xmlString">The XML string to validate.</param>
        /// <returns>true if the string is a well-formed XML; otherwise, false.</returns>
        public static bool IsValidXml(this string xmlString)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlString);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}
