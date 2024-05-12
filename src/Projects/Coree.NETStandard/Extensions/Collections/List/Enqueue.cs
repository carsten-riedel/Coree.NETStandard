using System;
using System.Collections.Generic;

namespace Coree.NETStandard.Extensions.Collections.List
{
    /// <summary>
    /// Contains extension methods for enhancing the functionality of the List class and other list-like collections.
    /// </summary>
    public static partial class CollectionsListExtensions
    {
        /// <summary>
        /// Adds an object to the end of the System.Collections.Generic.List`1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="item">The object to add to the System.Collections.Generic.List`1. The value can be null for reference types</param>
        /// <exception cref="NullReferenceException"></exception>
        public static void Enqueue<T>(this List<T> values, T item) where T : class
        {
            if (values != null)
            {
                values.Add(item);
            }
            else
            {
                throw new NullReferenceException();
            }
        }
    }
}