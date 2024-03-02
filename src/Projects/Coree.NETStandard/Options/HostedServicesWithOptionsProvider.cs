using Coree.NETStandard.Logging;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Options
{
    public interface IHostedServicesWithOptionsProvider<T>
    {
        void Enqueue(T item);

        T NextOption();
    }

    public class HostedServicesWithOptionsProvider<T> : IHostedServicesWithOptionsProvider<T> where T : new()
    {
        private ConcurrentQueue<T> OptionsQueue { get; set; } = new ConcurrentQueue<T>();

        public void Enqueue(T item)
        {
            OptionsQueue.Enqueue(item);
        }

        public T NextOption()
        {
            Log.Logger.ForContext<HostedServicesWithOptionsProvider<T>>().Verbose("verboselogging");
            var result = OptionsQueue.TryDequeue(out T? resultItem);
            if (!result)
            {
                return new T();
            }
            return resultItem;
        }
    }
}
