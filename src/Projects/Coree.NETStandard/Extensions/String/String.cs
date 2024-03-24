using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Coree.NETStandard.Extensions.Strings
{
    public static partial class StringExtensions
    {

        public static string[] SplitWith(this string input, string delimiter)
        {
            string escapedDelimiter = Regex.Escape(delimiter);
            return Regex.Split(input, escapedDelimiter);
        }

        public static string[] SplitWith(this string input, char[] delimiter)
        {
            var joined = string.Join("", delimiter);
            string escapedDelimiter = Regex.Escape(joined);
            return Regex.Split(input, escapedDelimiter);
        }

        public static string[] SplitWith(this string input, string[] delimiters)
        {
            string pattern = "(" + string.Join("|", delimiters.Select(Regex.Escape)) + ")";
            return Regex.Split(input, pattern);
        }

    }
}
