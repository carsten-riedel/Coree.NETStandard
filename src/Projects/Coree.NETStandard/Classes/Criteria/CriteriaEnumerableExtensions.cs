using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coree.NETStandard.Classes.Criteria
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
    public static partial class CriteriaEnumerableExtensions
    {
        public static IEnumerable<T> ApplyCriteria<T, TValue>(this IEnumerable<T> source, CriteriaItem<TValue> filterExpression)
        {
            return source.ApplyCriteria(new CriteriaItems(filterExpression));
        }

        public static IEnumerable<T> ApplyCriteria<T>(this IEnumerable<T> source, CriteriaItems filterExpressionGroup)
        {
            var filterExpression = CriteriaExpressionBuilder.BuildExpression<T>(filterExpressionGroup);

            if (filterExpression != null)
                return source.Where(filterExpression.Compile());
            return source;
        }
    }
}
