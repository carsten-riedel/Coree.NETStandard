using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coree.NETStandard.Extensions.Collections.Enumerable
{
    /// <summary>
    /// Provides extension methods for <see cref="IEnumerable{T}"/> to enhance and simplify operations on collections.
    /// </summary>
    /// <remarks>
    /// This static class contains a collection of utility methods that extend the functionality of the
    /// <see cref="IEnumerable{T}"/> interface. These methods offer convenient ways to perform common operations
    /// on enumerable collections, such as filtering, transformation, and aggregation, with a focus on improving
    /// code readability and efficiency.
    /// This class is declared as partial to allow for easy extension and organization of its methods across multiple
    /// files, facilitating maintainability and scalability of the utility functions provided.
    /// </remarks>
    public static partial class CollectionsEnumerableExtensions
    {
        /// <summary>
        /// Determines whether the sequence contains a specific value, using a case-insensitive ordinal comparison.
        /// </summary>
        /// <param name="source">The source enumerable.</param>
        /// <param name="value">The string to locate in the sequence.</param>
        /// <returns>true if the source sequence contains an element that has the specified value; otherwise, false.</returns>
        public static bool ContainsOrdinalIgnoreCase(this IEnumerable<string> source, string value)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (value == null) throw new ArgumentNullException(nameof(value));

            return source.Any(s => string.Equals(s, value, StringComparison.OrdinalIgnoreCase));
        }
    }
}
