using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Options
{
    public interface IHostedServicesCollectionOptionsProvider<T>
    {
        void Enqueue(T item);

        T MoveNext();
    }

    public class HostedServicesCollectionOptionsProvider<T> : IHostedServicesCollectionOptionsProvider<T> where T : new()
    {
        private ConcurrentQueue<T> OptionsQueue { get; set; } = new ConcurrentQueue<T>();

        public void Enqueue(T item)
        {
            OptionsQueue.Enqueue(item);
        }

        public T MoveNext()
        {
            var result = OptionsQueue.TryDequeue(out T? resultItem);
            if (!result)
            {
                return new T();
            }
            return resultItem;
        }
    }
}
