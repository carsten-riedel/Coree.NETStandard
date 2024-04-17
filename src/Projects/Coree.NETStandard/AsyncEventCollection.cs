using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Classes;

namespace Coree.NETStandard
{
    public static class EventSubscriptionHelper
    {


        // Generalize the methods to handle any delegate type that is a delegate
        public static void AddHandler<TDelegate>(ref TDelegate handlerField, TDelegate handlerToAdd) where TDelegate : Delegate
        {
            TDelegate currentHandler;
            TDelegate newHandler;
            do
            {
                currentHandler = handlerField;
                newHandler = (TDelegate)Delegate.Combine(currentHandler, handlerToAdd);
            }
            while (Interlocked.CompareExchange(ref handlerField, newHandler, currentHandler) != currentHandler);
        }

        public static void RemoveHandler<TDelegate>(ref TDelegate handlerField, TDelegate handlerToRemove) where TDelegate : Delegate
        {
            TDelegate currentHandler;
            TDelegate newHandler;
            do
            {
                currentHandler = handlerField;
                newHandler = (TDelegate)Delegate.Remove(currentHandler, handlerToRemove);
            }
            while (Interlocked.CompareExchange(ref handlerField, newHandler, currentHandler) != currentHandler);
        }
    }

    public class AsyncEventCollection<T>
    {
        public ThreadSafeCollection<T> Collection { get; set; } = new ThreadSafeCollection<T>();

        public delegate Task CustomEventDelegate(object sender, ThreadSafeCollection<T> collection, CancellationToken cancellationToken, string dispatchKey);

        private CustomEventDelegate _eventHandlers;

        public event CustomEventDelegate EventHandlers
        {
            add { EventSubscriptionHelper.AddHandler(ref _eventHandlers, value); }
            remove { EventSubscriptionHelper.RemoveHandler(ref _eventHandlers, value); }
        }

        public void DispatchEvents(CancellationToken cancellationToken = default, string dispatchKey = "")
        {
            var eventHandlers = _eventHandlers;
            if (eventHandlers != null)
            {
                foreach (var handler in eventHandlers.GetInvocationList().Cast<CustomEventDelegate>())
                {
                    // Pass the dispatch key to the handler
                    var _ = handler.Invoke(this, Collection, cancellationToken, dispatchKey);
                }
            }
        }
    }

}