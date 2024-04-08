using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Coree.NETStandard.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="System.Threading.CancellationToken"/> to enhance asynchronous programming patterns.
    /// </summary>
    public static partial class CancellationTokenExtensions
    {
        /// <summary>
        /// Creates a Task that completes when the specified <see cref="System.Threading.CancellationToken"/> is cancelled.
        /// This method facilitates integrating cancellation tokens with asynchronous operations, enabling easy response to cancellation requests in a Task-based pattern.
        /// <br/><br/>
        /// Example usage (awaiting application start in a background service):
        /// <code>
        /// public class MyBackgroundService : BackgroundService
        /// {
        ///     private readonly IHostApplicationLifetime _appLifetime;
        /// 
        ///     public MyBackgroundService(IHostApplicationLifetime appLifetime)
        ///     {
        ///         _appLifetime = appLifetime;
        ///     }
        /// 
        ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        ///     {
        ///         // Wait for the application to fully start
        ///         await _appLifetime.ApplicationStarted.WaitForCancellationAsync();
        /// 
        ///         Console.WriteLine("Application has started. Background service is proceeding with its tasks.");
        /// 
        ///         // Now that the application has started, proceed with the background service's main logic
        ///         while (!stoppingToken.IsCancellationRequested)
        ///         {
        ///             // Example operation: perform some work and wait
        ///             Console.WriteLine("Background service is doing its work...");
        ///             await Task.Delay(10000, stoppingToken); // Simulate some work by delaying
        ///         }
        ///     }
        /// }
        /// </code>
        /// Note: This extension is particularly useful for synchronizing startup sequences in applications, such as ensuring background services wait for the entire application to start before proceeding with their tasks.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> to monitor for cancellation.</param>
        /// <returns>A Task that completes when the token is cancelled.</returns>
        public static Task WaitForCancellationAsync(this System.Threading.CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            // Register to signal the task completion source when the cancellation token is cancelled
            cancellationToken.Register(() => tcs.TrySetResult(true));

            if (cancellationToken.IsCancellationRequested)
            {
                tcs.TrySetResult(true);
            }

            return tcs.Task;
        }
    }

}
