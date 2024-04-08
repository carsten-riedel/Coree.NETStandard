using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Extensions
{

    /// <summary>
    /// Provides extension methods for the <see cref="System.Text.StringBuilder"/> class.
    /// </summary>
    public static partial class StringBuilderExtension
    {

        /// <summary>
        /// Appends a new line to the <see cref="StringBuilder"/> instance. If the provided string is not null, empty, or whitespace,
        /// it appends the string followed by a new line; otherwise, it only appends a new line.
        /// </summary>
        /// <param name="stringbuilder">The <see cref="StringBuilder"/> instance on which the method is called.</param>
        /// <param name="value">The string to append before the new line. If the string is null, empty, or consists only of white-space characters, only a new line is appended.</param>
        public static void AppendLineOrDefault(this StringBuilder stringbuilder, string? value)
        {
            if (System.String.IsNullOrWhiteSpace(value))
            {
                stringbuilder.AppendLine();
            }
            else
            {
                stringbuilder.AppendLine(value);
            }
        }
    }
}
