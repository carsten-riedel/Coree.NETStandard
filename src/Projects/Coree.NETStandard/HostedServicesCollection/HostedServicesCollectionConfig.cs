using System.Collections.Concurrent;

namespace Coree.NETStandard.HostedServicesCollection
{
    /// <summary>
    /// Defines a contract for a configuration collection for hosted services, allowing the enqueuing of configuration items
    /// and retrieval of the next configuration item.
    /// </summary>
    /// <typeparam name="T">The type of the configuration item.</typeparam>
    public interface IHostedServicesCollectionConfig<T>
    {
        /// <summary>
        /// Enqueues a new configuration item into the collection.
        /// </summary>
        /// <param name="item">The configuration item to be added to the queue.</param>
        void Enqueue(T item);

        /// <summary>
        /// Fetches the next configuration item from the queue, removing it from the collection.
        /// If the queue is empty, returns a new instance of the configuration item type.
        /// </summary>
        /// <returns>The next configuration item if available; otherwise, a new instance of the configuration item type.</returns>
        T FetchNextConfig();
    }

    /// <summary>
    /// Represents a thread-safe collection of configuration items for hosted services.
    /// This collection allows for the addition of configuration items and provides a method
    /// to fetch and remove items sequentially, supporting concurrent operations.
    /// </summary>
    /// <typeparam name="T">The type of the configuration items, which must have a parameterless constructor.</typeparam>
    public class HostedServicesCollectionConfig<T> : IHostedServicesCollectionConfig<T> where T : new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HostedServicesCollectionConfig{T}"/> class.
        /// </summary>
        public HostedServicesCollectionConfig()
        {
            OptionsQueue = new ConcurrentQueue<T>();
        }

        /// <summary>
        /// A thread-safe queue that stores the configuration items.
        /// </summary>
        private ConcurrentQueue<T> OptionsQueue { get; set; }

        /// <summary>
        /// Adds a new configuration item to the end of the queue.
        /// </summary>
        /// <param name="item">The configuration item to be enqueued.</param>
        public void Enqueue(T item)
        {
            OptionsQueue.Enqueue(item);
        }

        /// <summary>
        /// Attempts to remove and return the configuration item at the beginning of the queue.
        /// If the queue is empty, a new instance of the configuration item type is returned.
        /// </summary>
        /// <returns>The next configuration item if available; otherwise, a new instance of the configuration item type.</returns>
        public T FetchNextConfig()
        {
            var success = OptionsQueue.TryDequeue(out T? item);
            if (!success)
            {
                return new T();
            }
            return item;
        }
    }

}
