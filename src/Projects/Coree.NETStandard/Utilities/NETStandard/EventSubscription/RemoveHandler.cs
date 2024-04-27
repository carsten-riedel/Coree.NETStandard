using System;
using System.Threading;

namespace Coree.NETStandard.Utilities
{
    /// <summary>
    /// Provides methods for safe manipulation of event handlers using atomic operations.
    /// </summary>
    public static partial class EventSubscription
    {
        /// <summary>
        /// Removes an event handler from the handler field atomically. Safe for null values.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate for the event handler.</typeparam>
        /// <param name="handlerField">The field holding the delegate from which the handler will be removed. Can be null.</param>
        /// <param name="handlerToRemove">The event handler delegate to remove.</param>
        public static void RemoveHandler<TDelegate>(ref TDelegate? handlerField, TDelegate handlerToRemove) where TDelegate : Delegate?
        {
            TDelegate? currentHandler;
            TDelegate newHandler;
            do
            {
                currentHandler = handlerField;
                if (currentHandler == null)
                {
                    return; // If there's no current handler, there's nothing to remove.
                }
                newHandler = (TDelegate)Delegate.Remove(currentHandler, handlerToRemove);
            }
            while (Interlocked.CompareExchange(ref handlerField, newHandler, currentHandler) != currentHandler);
        }
    }
}