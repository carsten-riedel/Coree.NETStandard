using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Extensions.Bool
{
    /// <summary>
    /// Provides extension methods for working with nullable boolean values.
    /// </summary>
    public static class BoolExtensions
    {
        /// <summary>
        /// Checks if the nullable boolean value is true.
        /// </summary>
        /// <param name="nullableBool">The nullable boolean value.</param>
        /// <returns>True if the value is true; otherwise, false.</returns>
        public static bool IsTrue(this bool? nullableBool)
        {
            return nullableBool.HasValue && nullableBool.Value;
        }

        /// <summary>
        /// Checks if the nullable boolean value is false.
        /// </summary>
        /// <param name="nullableBool">The nullable boolean value.</param>
        /// <returns>True if the value is false; otherwise, false.</returns>
        public static bool IsFalse(this bool? nullableBool)
        {
            return nullableBool.HasValue && !nullableBool.Value;
        }

        /// <summary>
        /// Checks if the nullable boolean value is null.
        /// </summary>
        /// <param name="nullableBool">The nullable boolean value.</param>
        /// <returns>True if the value is null; otherwise, false.</returns>
        public static bool IsNull(this bool? nullableBool)
        {
            return !nullableBool.HasValue;
        }
    }

}
