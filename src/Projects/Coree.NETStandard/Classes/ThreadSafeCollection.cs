using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Text.Json;
using System.Collections;
using System.Linq;

namespace Coree.NETStandard.Classes
{
    /// <summary>
    /// Represents a thread-safe collection of objects that can be accessed by multiple threads concurrently.
    /// </summary>
    /// <remarks>
    /// This collection uses locking to ensure that its operations are thread-safe. The collection is implemented
    /// as a list where items can be added, removed, or retrieved in a manner that prevents race conditions
    /// and data corruption when accessed from multiple threads. Null values can be stored, depending on the type <typeparamref name="T"/>.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the collection. This type can be a class, including nullable reference types.</typeparam>
    public class ThreadSafeCollection<T> : IEnumerable<T?>
    {
        private readonly List<T?> items;
        private readonly object syncRoot = new object(); // Object used for synchronization

        /// <summary>
        /// Initializes a new instance of the ThreadSafeCollection class that is empty.
        /// </summary>
        /// <remarks>
        /// Creates an empty collection with no items. The collection is ready to be accessed by multiple threads
        /// using its thread-safe methods. It supports storing null values, if the type <typeparamref name="T"/> permits.
        /// </remarks>
        public ThreadSafeCollection()
        {
            items = new List<T?>();
        }

        #region Stack

        /// <summary>
        /// Adds an item to the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method ensures that the collection can be safely modified from multiple threads by synchronizing access.
        /// The item is added to the end of the collection. It accepts null values for types that allow it.
        /// </remarks>
        /// <param name="item">The item to add to the collection. Can be null for nullable reference types.</param>
        public void Push(T? item)
        {
            lock (syncRoot)
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Removes and returns the last item from the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method locks the collection to ensure thread safety during the remove operation. If the collection is empty,
        /// it throws an InvalidOperationException. This change ensures that the caller is explicitly aware of the empty state.
        /// </remarks>
        /// <returns>The last item of the collection if it exists; otherwise, throws an InvalidOperationException.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the collection is empty.</exception>
        public T? Pop()
        {
            lock (syncRoot)
            {
                if (items.Count == 0)
                {
                    throw new InvalidOperationException("Cannot perform Pop operation on an empty collection.");
                }

                var item = items[items.Count - 1];
                items.RemoveAt(items.Count - 1);
                return item;
            }
        }

        /// <summary>
        /// Returns the last item from the collection without removing it, in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method locks the collection to ensure thread safety during the retrieval operation. If the collection is empty,
        /// it throws an InvalidOperationException to notify the caller of the empty state, aligning with the behavior of Pop.
        /// </remarks>
        /// <returns>The last item of the collection if it exists; otherwise, throws an InvalidOperationException.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the collection is empty.</exception>
        public T? Peek()
        {
            lock (syncRoot)
            {
                if (items.Count == 0)
                {
                    throw new InvalidOperationException("Cannot perform Peek operation on an empty collection.");
                }

                return items[items.Count - 1];
            }
        }

        #endregion Stack

        #region Queue

        /// <summary>
        /// Adds an item to the end of the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method ensures that items can be added to the collection safely from multiple threads by synchronizing access.
        /// It functions as part of the thread-safe queue functionality, allowing items to be enqueued (added to the end).
        /// </remarks>
        /// <param name="item">The item to add to the collection. Can be null if the type <typeparamref name="T"/> allows it.</param>
        public void Enqueue(T? item)
        {
            lock (syncRoot)
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Removes and returns the item at the beginning of the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method locks the collection to ensure thread safety during the remove operation. It is part of the thread-safe queue functionality,
        /// providing a way to dequeue items (remove from the beginning). If the collection is empty, it throws an InvalidOperationException to
        /// enforce handling of the empty state condition by the caller.
        /// </remarks>
        /// <returns>The item at the beginning of the collection if it exists; otherwise, throws an InvalidOperationException.</returns>
        /// <exception cref="InvalidOperationException">Thrown when attempting to perform the Dequeue operation on an empty collection.</exception>
        public T? Dequeue()
        {
            lock (syncRoot)
            {
                if (items.Count == 0)
                {
                    throw new InvalidOperationException("Cannot perform Dequeue operation on an empty collection.");
                }
                var item = items[0];
                items.RemoveAt(0);
                return item;
            }
        }

        #endregion Queue

        #region Enumerator

        /// <summary>
        /// Performs a LINQ operation on a snapshot of the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method locks the collection, takes a snapshot, and then applies the specified LINQ operation.
        /// It ensures thread safety by preventing other operations from modifying the collection during execution.
        /// The operation is performed on a snapshot to avoid locking during the entire enumeration,
        /// but it means the operation does not reflect changes made to the collection after the snapshot is taken.
        /// </remarks>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="operation">A function representing the LINQ operation to be performed on the collection.</param>
        /// <returns>An <see cref="IEnumerable{TResult}"/> that contains the result of applying the LINQ operation on the collection snapshot.</returns>
        public IEnumerable<TResult> LockedLinq<TResult>(Func<IEnumerable<T?>, IEnumerable<TResult>> operation)
        {
            lock (syncRoot)
            {
                // Creating a snapshot to avoid locking during the entire enumeration
                var snapshot = items.ToList();
                return operation(snapshot).ToList();
            }
        }

        /// <summary>
        /// Performs a LINQ operation on a snapshot of the collection and returns a deep copy of the results in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Similar to <see cref="LockedLinq"/>, this method locks the collection and operates on a snapshot to ensure thread safety.
        /// After performing the specified LINQ operation, it creates a deep copy of the result using JSON serialization.
        /// This approach guarantees that the operation's results are completely isolated from the original items in the collection,
        /// providing additional safety against mutations.
        /// </remarks>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="operation">A function representing the LINQ operation to be performed on the collection.</param>
        /// <returns>An <see cref="IEnumerable{TResult}"/> that contains a deep copy of the result of applying the LINQ operation on the collection snapshot.</returns>
        public IEnumerable<TResult> LockedLinqDeepCopy<TResult>(Func<IEnumerable<T?>, IEnumerable<TResult>> operation)
        {
            lock (syncRoot)
            {
                // Creating a snapshot to avoid locking during the entire enumeration
                var snapshot = items.ToList();
                var operationResult = operation(snapshot).ToList();
                // Perform a deep copy using JSON serialization
                var serializedResult = JsonSerializer.Serialize(operationResult);
                var deepCopiedResult = JsonSerializer.Deserialize<List<TResult>>(serializedResult);
                return deepCopiedResult ?? new List<TResult>();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// This method creates a snapshot of the current state of the collection and returns an enumerator for this snapshot,
        /// allowing for safe iteration over the collection items even when other threads might be modifying the collection concurrently.
        /// Note that the snapshot is a shallow copy; thus, the enumeration reflects the collection's state at the moment of the snapshot's creation.
        /// </remarks>
        /// <returns><![CDATA[An IEnumerator<T?> that can be used to iterate through the collection.]]></returns>
        public IEnumerator<T?> GetEnumerator()
        {
            T?[] snapshot;
            lock (syncRoot)
            {
                snapshot = items.ToArray();
            }
            foreach (var item in snapshot)
            {
                yield return item;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <remarks>
        /// This non-generic version is provided for compatibility with the IEnumerable interface and operates
        /// in the same thread-safe manner as the generic version, creating a snapshot of the collection for safe iteration.
        /// </remarks>
        /// <returns>An IEnumerator that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion Enumerator

        #region List

        /// <summary>
        /// Gets and removes the item at the specified index from the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Synchronizes access to the collection to safely remove the item, supporting concurrent modifications.
        /// If the index is out of range, an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </remarks>
        /// <param name="index">The zero-based index of the item to fetch and remove.</param>
        /// <returns>The item that was removed from the collection.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        public T? TakeAt(int index)
        {
            lock (syncRoot)
            {
                if (index < 0 || index >= items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }

                T? item = items[index];
                items.RemoveAt(index);
                return item;
            }
        }

        /// <summary>
        /// Finds and removes the first item matching the given predicate from the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Synchronizes access to the collection to safely remove the item, supporting concurrent modifications.
        /// If no item matches the predicate, <c>null</c> is returned.
        /// </remarks>
        /// <param name="predicate">The predicate used to find the item to remove.</param>
        /// <returns>The item that was removed from the collection, or <c>null</c> if no item matched the predicate.</returns>
        public T? Take(Func<T?, bool> predicate)
        {
            lock (syncRoot)
            {
                var index = items.FindIndex(item => predicate(item));
                if (index == -1)
                {
                    return default(T);
                }

                T? item = items[index];
                items.RemoveAt(index);
                return item;
            }
        }

        /// <summary>
        /// Finds and removes all items matching the given predicate from the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Synchronizes access to the collection to safely remove the items, supporting concurrent modifications.
        /// Returns an empty enumerable if no items match the predicate.
        /// </remarks>
        /// <param name="predicate">The predicate used to find the items to remove.</param>
        /// <returns>An enumerable of all items that were removed from the collection.</returns>
        public IEnumerable<T?> TakeAll(Func<T?, bool> predicate)
        {
            lock (syncRoot)
            {
                var itemsToRemove = items.Where(predicate).ToList();
                foreach (var item in itemsToRemove)
                {
                    items.Remove(item);
                }
                return itemsToRemove;
            }
        }

        /// <summary>
        /// Adds an item to the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Synchronizes access to the collection to safely add the item, supporting concurrent modifications.
        /// </remarks>
        /// <param name="item">The item to be added to the collection. Can be null if the type <typeparamref name="T"/> permits.</param>
        public void Add(T? item)
        {
            lock (syncRoot)
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific item from the collection in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Synchronizes access to the collection to safely remove the item, supporting concurrent modifications.
        /// If the item is not found, no action is taken.
        /// </remarks>
        /// <param name="item">The item to remove from the collection. Can be null if the type <typeparamref name="T"/> permits.</param>
        public void Remove(T? item)
        {
            lock (syncRoot)
            {
                items.Remove(item);
            }
        }

        #endregion List

        /// <summary>
        /// Retrieves the item at the specified index in a thread-safe manner.
        /// </summary>
        /// <remarks>
        /// Accesses the collection in a synchronized context to ensure safe retrieval of the item.
        /// </remarks>
        /// <param name="index">The zero-based index of the item to retrieve.</param>
        /// <returns>The item at the specified index. Can be null if the item itself is null or if the type <typeparamref name="T"/> allows null values.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the specified index is outside the bounds of the collection.</exception>
        public T? GetItemAt(int index)
        {
            lock (syncRoot)
            {
                if (index < 0 || index >= items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                return items[index];
            }
        }

        /// <summary>
        /// Returns a deep copy of the item at the specified index, ensuring thread safety and isolation from the original collection.
        /// </summary>
        /// <remarks>
        /// This method locks the collection during the operation and uses JSON serialization to create a deep copy of the item,
        /// ensuring that modifications to the returned object do not affect the original item in the collection.
        /// Throws <see cref="ArgumentOutOfRangeException"/> if the index is out of the valid range.
        /// </remarks>
        /// <param name="index">The zero-based index of the item to copy.</param>
        /// <returns>A deep copy of the item at the specified index. Returns null if the original item is null.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        public T? GetItemCopyAt(int index)
        {
            lock (syncRoot)
            {
                if (index < 0 || index >= items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                var itemJson = JsonSerializer.Serialize(items[index]);
                return JsonSerializer.Deserialize<T?>(itemJson);
            }
        }

        /// <summary>
        /// Creates and returns a deep copy of the entire collection as a List of T.
        /// </summary>
        /// <remarks>
        /// Locks the collection to ensure thread safety during the copy process. This method uses JSON serialization to create
        /// deep copies of the items, providing isolation from the original items. Modifications to the returned list or its items
        /// will not affect the original collection.
        /// </remarks>
        /// <returns>A new List containing deep copies of the items in the collection. Returns an empty list if the collection is empty.</returns>
        public List<T?> GetCollectionCopyToList()
        {
            lock (syncRoot)
            {
                // Serialize the items list to a JSON string.
                string serializedItems = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
                // Deserialize the JSON string back into a new List<T?>, effectively creating deep copies of the items.
                var deserializedItems = JsonSerializer.Deserialize<List<T?>>(serializedItems);
                // Return the deserialized list or a new empty list if deserialization returns null.
                return deserializedItems ?? new List<T?>();
            }
        }

        /// <summary>
        /// Gets the number of items in the collection, ensuring thread safety.
        /// </summary>
        /// <value>The number of items currently in the collection. Access is synchronized to prevent race conditions.</value>
        public int Count
        {
            get
            {
                lock (syncRoot)
                {
                    return items.Count;
                }
            }
        }

        /// <summary>
        /// Gets or sets the item at the specified index in a thread-safe manner.
        /// </summary>
        /// <param name="index">The zero-based index of the item to get or set.</param>
        /// <returns>The item at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is outside the bounds of the collection.</exception>
        /// <remarks>
        /// Access to the item is synchronized to ensure thread safety. Setting an item acquires a lock to prevent race conditions.
        /// </remarks>
        public T? this[int index]
        {
            get
            {
                return GetItemAt(index);
            }
            set
            {
                lock (syncRoot)
                {
                    if (index < 0 || index >= items.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                    }
                    items[index] = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the collection contains any items, ensuring thread safety.
        /// </summary>
        /// <value>
        /// <c>true</c> if the collection contains one or more items; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property locks the collection to ensure that the check is performed in a thread-safe manner,
        /// preventing race conditions that could arise from concurrent modifications to the collection.
        /// </remarks>
        public bool HasItems
        {
            get
            {
                lock (syncRoot)
                {
                    return items.Count > 0;
                }
            }
        }
    }



}