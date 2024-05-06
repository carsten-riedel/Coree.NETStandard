using System.Collections.Generic;
using System.Linq;

namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Provides extension methods for <see cref="IEnumerable{T}"/> to enhance operations such as filtering, transforming, and aggregating collections.
    /// </summary>
    /// <remarks>
    /// Declared as a partial class, it allows for methods to be organized across multiple files, enhancing maintainability and scalability.
    /// </remarks>
    public static partial class CriteriaEnumerableExtensions
    {
        /// <summary>
        /// Applies specified criteria to filter a collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TValue">The type of the value used in the criteria.</typeparam>
        /// <param name="source">The source collection to filter.</param>
        /// <param name="filterExpression">The criteria to apply to the collection.</param>
        /// <returns>A filtered collection that matches the criteria.</returns>
        public static IEnumerable<T> ApplyCriteria<T, TValue>(this IEnumerable<T> source, CriteriaItem<TValue> filterExpression)
        {
            return source.ApplyCriteria(new CriteriaItems(filterExpression));
        }

        /// <summary>
        /// Applies a group of criteria to filter a collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
        /// <param name="source">The source collection to filter.</param>
        /// <param name="filterExpressionGroup">The group of criteria items defining the filtering rules.</param>
        /// <returns>A filtered collection that matches the combined criteria.</returns>
        /// <remarks>
        /// This method constructs a dynamic filter based on multiple criteria items and applies it to the source collection.
        /// If no valid filter expression can be built (e.g., if the criteria group is empty), the original source collection is returned unchanged.
        /// </remarks>
        public static IEnumerable<T> ApplyCriteria<T>(this IEnumerable<T> source, CriteriaItems filterExpressionGroup)
        {
            var filterExpression = CriteriaExpressionBuilder.BuildExpression<T>(filterExpressionGroup);

            if (filterExpression != null)
                return source.Where(filterExpression.Compile());
            return source;
        }
    }
}