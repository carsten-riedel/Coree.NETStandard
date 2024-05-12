namespace Coree.NETStandard.Classes.ThreadSafeValue
{
    /// <summary>
    /// Provides a thread-safe wrapper for a value of type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This class is designed to allow safe concurrent access to a value by multiple threads.
    /// It uses a locking mechanism to ensure that only one thread can read or modify the value at any time,
    /// preventing race conditions and ensuring consistency of the value across threads.
    /// The type <typeparamref name="T"/> can be either a value type or a reference type. If <typeparamref name="T"/> is a reference type.
    /// </remarks>
    /// <typeparam name="T">The type of the value to be stored. This can be any type, including nullable and non-nullable types.</typeparam>
    public class ThreadSafeValue<T>
    {
        private T? _value; // Indicates that _value can be null, respecting the nullability of type T.

        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeValue{T}"/> class, optionally with an initial value.
        /// </summary>
        /// <param name="initialValue">The initial value to store. Default is the default value of type <typeparamref name="T"/>,
        /// which is null for reference types and zero or the default constructor value for value types.</param>
        public ThreadSafeValue(T? initialValue = default)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Gets or sets the value in a thread-safe manner.
        /// </summary>
        /// <value>The current value stored within the <see cref="ThreadSafeValue{T}"/> instance.</value>
        /// <remarks>
        /// Access to the value is synchronized, ensuring that read and write operations are safe to use from multiple threads concurrently.
        /// The property getter returns the current value, and the property setter updates the value.
        /// Both operations are performed with thread safety in mind, using a private lock to synchronize access.
        /// </remarks>
        public T? Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
            set
            {
                lock (_lock)
                {
                    _value = value;
                }
            }
        }
    }
}