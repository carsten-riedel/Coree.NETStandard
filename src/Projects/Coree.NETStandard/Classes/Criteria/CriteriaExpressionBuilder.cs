using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Coree.NETStandard.Classes.Criteria
{
    /// <summary>
    /// Provides functionality to build dynamic LINQ expressions based on criteria items.
    /// </summary>
    public class CriteriaExpressionBuilder
    {
        /// <summary>
        /// Builds a dynamic LINQ expression based on the provided criteria group.
        /// </summary>
        /// <typeparam name="T">The type of the objects to filter.</typeparam>
        /// <param name="group">The criteria items defining the filtering rules.</param>
        /// <returns>An expression representing the combined criteria; or null if no filters are provided.</returns>
        /// <exception cref="InvalidOperationException">Thrown when a property specified in any criteria item does not exist on type <typeparamref name="T"/>.</exception>
        public static Expression<Func<T, bool>>? BuildExpression<T>(CriteriaItems group)
        {
            if (group.Filters.Length == 0)
                return null;

            ParameterExpression param = Expression.Parameter(typeof(T), "s");
            Expression? combinedExpression = null;

            // Validate all properties exist on type T
            foreach (var filter in group.Filters)
            {
                _ = typeof(T).GetProperty(filter.PropertyName) ?? throw new InvalidOperationException($"Property '{filter.PropertyName}' does not exist on type '{typeof(T).Name}'.");
            }

            // Construct the filter expressions and combine them based on their specified logical operators
            for (int i = 0; i < group.Filters.Length; i++)
            {
                var filter = group.Filters[i];
                MemberExpression property = Expression.Property(param, filter.PropertyName);
                ConstantExpression constant = Expression.Constant(filter.Value, property.Type);
                Expression conditionExpression;

                // Determine which type-specific method to call based on the property type
                if (property.Type == typeof(string))
                {
                    conditionExpression = HandleStringConditions(property, filter, constant);
                }
                else if (property.Type == typeof(int))
                {
                    conditionExpression = HandleIntConditions(property, filter, constant);
                }
                else if (property.Type == typeof(DateTime))
                {
                    conditionExpression = HandleDateTimeConditions(property, filter, constant);
                }
                else if (property.Type == typeof(decimal))
                {
                    conditionExpression = HandleDecimalConditions(property, filter, constant);
                }
                else
                {
                    throw new InvalidOperationException("Unsupported property type for filtering.");
                }

                if (filter.Negate)
                    conditionExpression = Expression.Not(conditionExpression);

                // Apply logical operator
                if (combinedExpression == null)
                {
                    combinedExpression = conditionExpression;
                }
                else
                {
                    if (filter.Operator == CriteriaOperator.And)
                    {
                        combinedExpression = Expression.AndAlso(combinedExpression, conditionExpression);
                    }
                    else if (filter.Operator == CriteriaOperator.Or)
                    {
                        combinedExpression = Expression.OrElse(combinedExpression, conditionExpression);
                    }
                }
            }

            return Expression.Lambda<Func<T, bool>>(combinedExpression, param);
        }

        /// <summary>
        /// Handles conditions specific to string properties based on the comparison method specified in the filter.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="filter">The criteria defining the filtering rule.</param>
        /// <param name="constant">The value to compare against.</param>
        /// <param name="ignoreCase">Indicates whether the string comparison should ignore case.</param>
        /// <returns>The expression created based on the string comparison method.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unsupported string comparison method is used.</exception>
        private static Expression HandleStringConditions(MemberExpression property, CriteriaItem<object> filter, ConstantExpression constant, bool ignoreCase = true)
        {
            MethodInfo methodInfo;
            StringComparison comparisonType = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            switch (filter.ComparisonMethod)
            {
                case CriteriaComparisonMethod.IsNullOrWhiteSpace:
                    // IsNullOrWhiteSpace does not need to consider case sensitivity
                    return Expression.Call(typeof(string).GetMethod("IsNullOrWhiteSpace", new[] { typeof(string) }), property);

                case CriteriaComparisonMethod.Equals:
                    // Use StringComparison for case-insensitive comparison
                    methodInfo = typeof(string).GetMethod("Equals", new[] { typeof(string), typeof(StringComparison) });
                    return Expression.Call(property, methodInfo, constant, Expression.Constant(comparisonType));

                case CriteriaComparisonMethod.Contains:
                    methodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string), typeof(StringComparison) });
                    return Expression.Call(property, methodInfo, constant, Expression.Constant(comparisonType));

                case CriteriaComparisonMethod.StartsWith:
                    methodInfo = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(StringComparison) });
                    return Expression.Call(property, methodInfo, constant, Expression.Constant(comparisonType));

                case CriteriaComparisonMethod.EndsWith:
                    methodInfo = typeof(string).GetMethod("EndsWith", new[] { typeof(string), typeof(StringComparison) });
                    return Expression.Call(property, methodInfo, constant, Expression.Constant(comparisonType));

                default:
                    throw new InvalidOperationException("Unsupported string comparison method.");
            }
        }

        /// <summary>
        /// Handles conditions specific to integer properties, supporting a range of comparison operations.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="filter">The criteria defining the filtering rule.</param>
        /// <param name="constant">The value to compare against.</param>
        /// <returns>The expression created based on the integer comparison method.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unsupported comparison method is used for integer properties.</exception>
        private static Expression HandleIntConditions(MemberExpression property, CriteriaItem<object> filter, ConstantExpression constant)
        {
            switch (filter.ComparisonMethod)
            {
                case CriteriaComparisonMethod.Equals:
                    return Expression.Equal(property, constant);
                case CriteriaComparisonMethod.GreaterThan:
                    return Expression.GreaterThan(property, constant);
                case CriteriaComparisonMethod.LessThan:
                    return Expression.LessThan(property, constant);
                case CriteriaComparisonMethod.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(property, constant);
                case CriteriaComparisonMethod.LessThanOrEqual:
                    return Expression.LessThanOrEqual(property, constant);
                case CriteriaComparisonMethod.Contains:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for integer properties.");
                case CriteriaComparisonMethod.StartsWith:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for integer properties.");
                case CriteriaComparisonMethod.EndsWith:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for integer properties.");
                case CriteriaComparisonMethod.IsNullOrWhiteSpace:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for integer properties.");
                default:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for integer properties.");
            }
        }


        /// <summary>
        /// Handles conditions specific to DateTime properties, interpreting date-only equality as a range.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="filter">The criteria defining the filtering rule.</param>
        /// <param name="constant">The value to compare against.</param>
        /// <returns>The expression created based on the DateTime comparison method.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unsupported comparison method is used for DateTime properties.</exception>
        private static Expression HandleDateTimeConditions(MemberExpression property, CriteriaItem<object> filter, ConstantExpression constant)
        {
            DateTime dateValue = (DateTime)constant.Value;
            ConstantExpression dateOnlyConstant = Expression.Constant(dateValue.Date, typeof(DateTime));
            ConstantExpression nextDayConstant = Expression.Constant(dateValue.Date.AddDays(1), typeof(DateTime));

            switch (filter.ComparisonMethod)
            {
                case CriteriaComparisonMethod.Equals:
                    // Creates a range check for the entire day
                    Expression dateAtOrAfter = Expression.GreaterThanOrEqual(property, dateOnlyConstant);
                    Expression dateBeforeNextDay = Expression.LessThan(property, nextDayConstant);
                    return Expression.AndAlso(dateAtOrAfter, dateBeforeNextDay);

                case CriteriaComparisonMethod.GreaterThan:
                    return Expression.GreaterThan(property, dateOnlyConstant);

                case CriteriaComparisonMethod.LessThan:
                    return Expression.LessThan(property, nextDayConstant); // Ensures it is less than the start of the next day

                case CriteriaComparisonMethod.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(property, dateOnlyConstant);

                case CriteriaComparisonMethod.LessThanOrEqual:
                    // Adjusted to include the entire day
                    return Expression.LessThan(property, nextDayConstant);

                default:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for DateTime properties.");
            }
        }

        /// <summary>
        /// Handles conditions specific to decimal properties, supporting a range of comparison operations.
        /// </summary>
        /// <param name="property">The property to filter on.</param>
        /// <param name="filter">The criteria defining the filtering rule.</param>
        /// <param name="constant">The value to compare against.</param>
        /// <returns>The expression created based on the decimal comparison method.</returns>
        /// <exception cref="InvalidOperationException">Thrown when an unsupported comparison method is used for decimal properties.</exception>
        private static Expression HandleDecimalConditions(MemberExpression property, CriteriaItem<object> filter, ConstantExpression constant)
        {
            // Ensure the constant is of type decimal
            ConstantExpression adjustedConstant = Expression.Constant(Convert.ToDecimal(constant.Value), typeof(decimal));

            switch (filter.ComparisonMethod)
            {
                case CriteriaComparisonMethod.Equals:
                    return Expression.Equal(property, adjustedConstant);
                case CriteriaComparisonMethod.GreaterThan:
                    return Expression.GreaterThan(property, adjustedConstant);
                case CriteriaComparisonMethod.LessThan:
                    return Expression.LessThan(property, adjustedConstant);
                case CriteriaComparisonMethod.GreaterThanOrEqual:
                    return Expression.GreaterThanOrEqual(property, adjustedConstant);
                case CriteriaComparisonMethod.LessThanOrEqual:
                    return Expression.LessThanOrEqual(property, adjustedConstant);
                default:
                    throw new InvalidOperationException($"Unsupported comparison method '{filter.ComparisonMethod}' for decimal properties.");
            }
        }

    }
}