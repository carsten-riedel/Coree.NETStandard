using System.Linq;

namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Provides extension methods for <see cref="IQueryable{T}"/> to apply filtering criteria dynamically.
    /// </summary>
    public static partial class CriteriaQueryableExtensions
    {
        /// <summary>
        /// Applies specified criteria to filter an <see cref="IQueryable{T}"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
        /// <typeparam name="TValue">The type of the value used in the criteria.</typeparam>
        /// <param name="source">The source collection to filter.</param>
        /// <param name="filterExpression">The single criteria item to apply to the collection.</param>
        /// <returns>A filtered <see cref="IQueryable{T}"/> that matches the criteria.</returns>
        public static IQueryable<T> ApplyCriteria<T, TValue>(this IQueryable<T> source, CriteriaItem<TValue> filterExpression)
        {
            return source.ApplyCriteria(new CriteriaItems(filterExpression));
        }

        /// <summary>
        /// Applies a group of criteria to filter an <see cref="IQueryable{T}"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source collection.</typeparam>
        /// <param name="source">The source collection to filter.</param>
        /// <param name="filterExpressionGroup">The group of criteria items defining the filtering rules.</param>
        /// <returns>A filtered <see cref="IQueryable{T}"/> that matches the combined criteria.</returns>
        /// <remarks>
        /// Constructs a dynamic expression based on the criteria items and applies it to the source collection.
        /// If the expression is null (e.g., if the criteria group is empty or invalid), the original collection is returned unchanged.
        /// </remarks>
        public static IQueryable<T> ApplyCriteria<T>(this IQueryable<T> source, CriteriaItems filterExpressionGroup)
        {
            var filterExpression = CriteriaExpressionBuilder.BuildExpression<T>(filterExpressionGroup);
            if (filterExpression != null)
                return source.Where(filterExpression);
            return source;
        }
    }
}