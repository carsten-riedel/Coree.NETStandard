using System.Collections.Generic;

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
        /// Filters out null entries from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <remarks>
        /// This extension method iterates over each element in the given enumerable,
        /// returning only those elements that are not null. It is applicable to collections of reference types.
        /// </remarks>
        /// <typeparam name="T">The type of elements in the collection, constrained to reference types.</typeparam>
        /// <param name="enumerable">The enumerable collection to filter.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> without null elements.</returns>
        public static IEnumerable<T> RemoveNullEntries<T>(this IEnumerable<T?> enumerable) where T : class?
        {
            foreach (var item in enumerable)
            {
                if (item != null)
                {
                    yield return item;
                }
            }
        }
    }
}