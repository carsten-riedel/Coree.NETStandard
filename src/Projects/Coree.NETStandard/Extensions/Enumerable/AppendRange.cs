using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Extensions
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
    public static partial class EnumerableTExtension
    {
        /// <summary>
        /// Appends a range of items to the end of an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of the elements in both the source and items collections. Must be a class.</typeparam>
        /// <param name="source">The source collection to which the items will be appended.</param>
        /// <param name="items">The collection of items to append to the source collection.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that consists of the original elements followed by the added items.</returns>
        /// <exception cref="ArgumentNullException">Thrown if either the source collection or the items collection is null.</exception>
        public static IEnumerable<T> AppendRange<T>(this IEnumerable<T> source, IEnumerable<T> items) where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source collection is null.");
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items), "Items collection is null.");
            }

            // Yield all elements in the original collection
            foreach (var element in source)
            {
                yield return element;
            }

            // Yield all elements from the items collection
            foreach (var item in items)
            {
                yield return item;
            }
        }
    }
}
