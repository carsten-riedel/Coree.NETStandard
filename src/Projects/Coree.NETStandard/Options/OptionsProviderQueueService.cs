using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coree.NETStandard.Options
{
    public interface IOptionsProviderQueue<T>
    {
        void Enqueue(T item);
        T Dequeue();
        void Add(T item);
        IEnumerator<T> GetEnumerator();
    }

    public class OptionsProviderQueueService<T> : IOptionsProviderQueue<T>, IEnumerable<T> where T : new()
    {
        private ConcurrentQueue<T> OptionsQueue { get; set; } = new ConcurrentQueue<T>();

        public OptionsProviderQueueService()
        {
        }

        public void Enqueue(T item)
        {
            OptionsQueue.Enqueue(item);
        }

        public T Dequeue()
        {
            var result = OptionsQueue.TryDequeue(out T? resultItem);
            if (!result)
            {
                return new T();
            }
            return resultItem;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return OptionsQueue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Enqueue(item);
        }
    }
}
