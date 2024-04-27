using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Coree.NETStandard.Classes.Criteria
{
    public static partial class CriteriaQueryableExtensions
    {
        public static IQueryable<T> ApplyCriteria<T, TValue>(this IQueryable<T> source, Classes.Criteria.CriteriaItem<TValue> filterExpression)
        {
            return source.ApplyCriteria(new Classes.Criteria.CriteriaItems(filterExpression));
        }

        public static IQueryable<T> ApplyCriteria<T>(this IQueryable<T> source, Classes.Criteria.CriteriaItems filterExpressionGroup)
        {
            var filterExpression = Classes.Criteria.CriteriaExpressionBuilder.BuildExpression<T>(filterExpressionGroup);
            if (filterExpression != null)
                return source.Where(filterExpression);
            return source;
        }
    }
}
