using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Coree.NETStandard.Extensions.Enumerable
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
        public enum StringComparisonMethod
        {
            Contains,
            StartsWith,
            EndsWith
        }


        /// <summary>
        /// Filters a sequence of values based on a predicate. Optionally negates the predicate.
        /// </summary>
        /// <param name="source">An IEnumerable to filter.</param>
        /// <param name="propertyName">The name of the property to test.</param>
        /// <param name="comparisonMethod">The comparison method to use.</param>
        /// <param name="value">The value to compare with the property.</param>
        /// <param name="negate">True to negate the predicate; false to use it as is.</param>
        /// <returns>An IEnumerable that contains elements from the input sequence that satisfy the condition.</returns>
        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, string propertyName, StringComparisonMethod comparisonMethod, string value, bool negate = false)
        {
            // Create the parameter expression for the type parameter
            ParameterExpression param = Expression.Parameter(typeof(T), "s");

            // Access the property specified by propertyName
            MemberExpression property = Expression.Property(param, propertyName);

            // Find the appropriate method in the String class
            MethodInfo methodInfo = typeof(string).GetMethod(comparisonMethod.ToString(), new[] { typeof(string) });

            // Create the constant expression for the comparison value
            ConstantExpression constant = Expression.Constant(value, typeof(string));

            // Create the method call expression for the string method
            MethodCallExpression methodCall = Expression.Call(property, methodInfo, constant);

            // Optionally negate the expression
            Expression finalExpression = negate ? Expression.Not(methodCall) : (Expression)methodCall;

            // Create the lambda expression and compile it
            var lambda = Expression.Lambda<Func<T, bool>>(finalExpression, param).Compile();

            // Return the filtered sequence
            return source.Where(lambda);
        }
    }
}