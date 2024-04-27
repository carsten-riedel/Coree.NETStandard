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
        /// Adds an event handler to the handler field atomically. Safe for null initial values.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate for the event handler.</typeparam>
        /// <param name="handlerField">The field holding the delegate to which the new handler will be added. Can be null initially.</param>
        /// <param name="handlerToAdd">The event handler delegate to add.</param>
        public static void AddHandler<TDelegate>(ref TDelegate? handlerField, TDelegate handlerToAdd) where TDelegate : Delegate?
        {
            TDelegate? currentHandler;
            TDelegate newHandler;
            do
            {
                currentHandler = handlerField;
                newHandler = (TDelegate)Delegate.Combine(currentHandler, handlerToAdd);
            }
            while (Interlocked.CompareExchange(ref handlerField, newHandler, currentHandler) != currentHandler);
        }
    }

}
