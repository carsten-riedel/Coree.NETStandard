using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Extensions.Collections.List
{

    /// <summary>
    /// Contains extension methods for enhancing the functionality of the List class and other list-like collections.
    /// </summary>
    public static partial class CollectionsListExtensions
    {
        /// <summary>
        /// Removes and returns the object at the beginning of the System.Collections.Generic.List`1.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns>The object that is removed from the beginning of the System.Collections.List.List`1.</returns>
        /// <exception cref="InvalidOperationException">The System.Collections.Generic.List`1 is empty</exception>
        /// <exception cref="NullReferenceException"></exception>
        public static T? Dequeue<T>(this List<T> values) where T : class
        {
            T? retval = null;
            if (values != null && values.Count >= 1)
            {
                retval = values[0];
                values.RemoveAt(0);
            }
            else if (values != null && values.Count == 0)
            {
                throw new InvalidOperationException("Queue empty.");

            }
            else if (values == null)
            {
                throw new NullReferenceException();
            }

            return retval;
        }
    }
}
