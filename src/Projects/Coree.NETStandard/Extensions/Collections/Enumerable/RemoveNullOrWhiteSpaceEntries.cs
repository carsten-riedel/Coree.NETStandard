﻿using System.Collections.Generic;

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
        /// Filters out null, empty, and whitespace-only entries from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <remarks>
        /// This extension method iterates over each element in the given enumerable,
        /// returning only those elements that are not null, not empty, and not made up solely of whitespace characters.
        /// </remarks>
        /// <param name="enumerable">The enumerable collection of strings to filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> consisting of non-empty and non-whitespace strings.</returns>
        public static IEnumerable<string> RemoveNullOrWhiteSpaceEntries(this IEnumerable<string?> enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item != null && !string.IsNullOrWhiteSpace(item))
                {
                    yield return item;
                }
            }
        }
    }
}