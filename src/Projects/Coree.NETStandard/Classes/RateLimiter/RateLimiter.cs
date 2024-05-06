using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Coree.NETStandard.Classes.RateLimiter
{


    public class RateLimit<T>
    {
        private readonly int _maxOperations;
        private readonly TimeSpan _period;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<Func<Task>> _taskQueue;
        private readonly Timer _timer;

        public RateLimit(int maxOperations, TimeSpan period)
        {
            _maxOperations = maxOperations;
            _period = period;
            _semaphore = new SemaphoreSlim(maxOperations, maxOperations);
            _taskQueue = new ConcurrentQueue<Func<Task>>();

            // Timer to refill semaphore slots and attempt queued task execution
            _timer = new Timer(ExecuteQueuedTasks, null, period, period);
        }

        public async Task<T> EnqueueTask(Func<Task<T>> taskFunc)
        {
            var tcs = new TaskCompletionSource<T>();

            _taskQueue.Enqueue(async () =>
            {
                try
                {
                    // Wait to acquire the semaphore before executing the task
                    await _semaphore.WaitAsync();
                    var result = await taskFunc();
                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
                finally
                {
                    _semaphore.Release();
                }
            });

            TryExecuteTask();
            return await tcs.Task;
        }

        private void ExecuteQueuedTasks(object state)
        {
            // Try to execute as many tasks as possible within the current rate limit
            while (_semaphore.CurrentCount > 0 && _taskQueue.TryDequeue(out var task))
            {
                Task.Run(task);
            }
        }

        private void TryExecuteTask()
        {
            if (_taskQueue.TryDequeue(out var task))
            {
                Task.Run(task);
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            _timer.Dispose();
        }
    }


}
