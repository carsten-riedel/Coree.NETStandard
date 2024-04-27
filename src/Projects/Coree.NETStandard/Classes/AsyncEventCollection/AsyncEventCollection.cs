using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Coree.NETStandard.Utilities;
using Coree.NETStandard.Classes.ThreadSafeCollection;

namespace Coree.NETStandard.Classes.AsyncEventCollection
{
    /// <summary>
    /// Manages a collection of events and provides asynchronous event dispatching.
    /// </summary>
    /// <typeparam name="T">The type of elements stored in the thread-safe collection.</typeparam>
    public class AsyncEventCollection<T>
    {
        /// <summary>
        /// Gets or sets the thread-safe collection of elements.
        /// </summary>
        public ThreadSafeCollection<T> Collection { get; set; } = new ThreadSafeCollection<T>();

        /// <summary>
        /// Defines a delegate for custom event handlers that support asynchronous operations.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="collection">The thread-safe collection associated with the event.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the event handling.</param>
        /// <param name="dispatchKey">A key used to identify the dispatch operation.</param>
        public delegate Task CustomEventDelegate(object sender, ThreadSafeCollection<T> collection, CancellationToken cancellationToken, string dispatchKey);

        private CustomEventDelegate? _eventHandlers;

        /// <summary>
        /// Event that can be subscribed to in order to receive asynchronous notifications when the collection changes.
        /// </summary>
        public event CustomEventDelegate EventHandlers
        {
            add { EventSubscription.AddHandler(ref _eventHandlers, value); }
            remove { EventSubscription.RemoveHandler(ref _eventHandlers, value); }
        }

        /// <summary>
        /// Dispatches events to all subscribed handlers asynchronously. If no handlers are subscribed, the method completes without action.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token to cancel the dispatch operation.</param>
        /// <param name="dispatchKey">Optional key to provide context or identification for the event dispatch.</param>
        public void DispatchEvents(CancellationToken cancellationToken = default, string dispatchKey = "")
        {
            var eventHandlers = _eventHandlers;
            if (eventHandlers != null)
            {
                foreach (var handler in eventHandlers.GetInvocationList().Cast<CustomEventDelegate>())
                {
                    // This invokes each handler in an asynchronous manner and does not await the task, hence fire-and-forget.
                    var _ = handler.Invoke(this, Collection, cancellationToken, dispatchKey);
                }
            }
        }
    }

}
