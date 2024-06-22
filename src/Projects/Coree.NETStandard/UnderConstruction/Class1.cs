using System;
using System.Collections.Immutable;
using System.Net;
using System.Threading;

#pragma warning disable

namespace Coree.NETStandard.UnderConstruction
{

    public class TimedEntry<T>
    {
        public T Value { get; }
        public DateTime RecordedAt { get; }

        public TimedEntry(T value, DateTime recordedAt)
        {
            Value = value;
            RecordedAt = recordedAt;
        }
    }

    public class LimitedHistory<T> : IDisposable
    {
        private ImmutableList<TimedEntry<T>> entries = ImmutableList<TimedEntry<T>>.Empty;
        private readonly int maxCapacity;
        private readonly TimeSpan maxEntryAge;
        private readonly Timer cleanupTimer;
        private bool isDisposed = false;

        public LimitedHistory(int capacity = 100, TimeSpan entryAgeLimit = default, TimeSpan cleanupInterval = default)
        {
            maxCapacity = capacity;

            if (entryAgeLimit == default)
                entryAgeLimit = TimeSpan.FromMinutes(1); // Default age limit set to 1 minute if not specified

            if (cleanupInterval == default)
                cleanupInterval = TimeSpan.FromSeconds(5); // Default cleanup interval set to 1 minute if not specified

            maxEntryAge = entryAgeLimit;

            cleanupTimer = new Timer(_ => TriggerCleanup(), null, cleanupInterval, cleanupInterval);
        }

        public void AddEntry(T value)
        {
            var newEntry = new TimedEntry<T>(value, DateTime.Now);

            ImmutableList<TimedEntry<T>> currentEntries, updatedEntries;
            do
            {
                currentEntries = entries;
                updatedEntries = currentEntries.Add(newEntry);
            }
            while (Interlocked.CompareExchange(ref entries, updatedEntries, currentEntries) != currentEntries);

            MaintainCapacity();
        }

        private void TriggerCleanup()
        {
            var now = DateTime.Now;
            ImmutableList<TimedEntry<T>> currentEntries, updatedEntries;
            do
            {
                currentEntries = entries;
                updatedEntries = currentEntries;
                updatedEntries = updatedEntries.RemoveAll(e => now - e.RecordedAt > maxEntryAge);

            } while (Interlocked.CompareExchange(ref entries, updatedEntries, currentEntries) != currentEntries);
        }

        private void MaintainCapacity()
        {
            ImmutableList<TimedEntry<T>> currentEntries, updatedEntries;
            do
            {
                currentEntries = entries;
                updatedEntries = currentEntries;

                // Ensure the list does not exceed the maximum capacity
                while (updatedEntries.Count > maxCapacity)
                    updatedEntries = updatedEntries.RemoveAt(0); // Removes from the start

            } while (Interlocked.CompareExchange(ref entries, updatedEntries, currentEntries) != currentEntries);
        }

        public ImmutableList<TimedEntry<T>> GetEntries()
        {
            return entries;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    cleanupTimer.Dispose();
                }
                isDisposed = true;
            }
        }

        ~LimitedHistory()
        {
            Dispose(false);
        }
    }



}

#pragma warning restore
