using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Abstractions.InitializationService
{
    /// <summary>
    /// Represents an abstract base class for a service that must complete its initialization before other services can start.
    /// This service ensures all setup tasks are completed and signals other dependent services when it is safe to begin their processes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example usage:
    /// <code>
    /// services.AddHostedService&lt;MyStartup&gt;();
    /// services.AddHostedService&lt;MyDependend&gt;();
    /// 
    /// public class MyStartup : InitializationService
    /// {
    ///    private readonly ILogger&lt;MyStartup&gt; _logger;
    ///    public MyStartup(ILogger&lt;MyStartup&gt; logger) { _logger = logger; }
    ///
    ///    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///    {
    ///       _logger.LogInformation("Start MyStartup");
    ///       try { await Task.Delay(5000, stoppingToken); }
    ///       catch (TaskCanceledException) { _logger.LogInformation("MyStartup was canceled."); return; }
    ///       _logger.LogInformation("End MyStartup");
    ///    }
    /// }
    /// 
    /// public class MyDependend : InitializationDependentServices
    /// {
    ///    private readonly ILogger&lt;MyDependend&gt; _logger;
    ///    public MyDependend(ILogger&lt;MyDependend&gt; logger) { _logger = logger; }
    ///
    ///    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///    {
    ///       _logger.LogInformation("Start MyDependend");
    ///       try { await Task.Delay(5000, stoppingToken); }
    ///       catch (TaskCanceledException) { _logger.LogInformation("MyDependend was canceled."); return; }
    ///       _logger.LogInformation("End MyDependend");
    ///    }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public abstract class InitializationService : IHostedService, IDisposable
    {
        private static bool _ready = false; // Static flag to indicate readiness.
        private static readonly object _lock = new object(); // Lock object for synchronization.

        private Task? _executeTask;  // Task executing the initialization logic.
        private CancellationTokenSource? _stoppingCts; // Cancellation token source for stopping the service.

        /// <summary>
        /// Gets the task that is executing the initialization logic, or null if no task has been started.
        /// </summary>
        public virtual Task? ExecuteTask => _executeTask;

        /// <summary>
        /// When overridden in a derived class, executes the initialization logic as an asynchronous operation.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token that should be used to request cancellation of the initialization task.</param>
        /// <returns>A task that represents the asynchronous initialization operation.</returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Waits for the initialization to complete. This method blocks until the initialization service signals that it is ready.
        /// </summary>
        public static void WaitForRelease()
        {
            lock (_lock)
            {
                while (!_ready)
                    Monitor.Wait(_lock);
            }
        }

        /// <summary>
        /// Signals that the initialization is complete and dependent services can start.
        /// This method should be called once the initialization tasks are finished.
        /// </summary>
        public static void ReleaseHold()
        {
            lock (_lock)
            {
                _ready = true;
                Monitor.PulseAll(_lock);
            }
        }

        /// <summary>
        /// Starts the service initialization process.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The cancellation token should cancel the initialization process.</param>
        /// <returns>A task that represents the asynchronous operation of starting the service.</returns>
        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _executeTask = ExecuteAsync(_stoppingCts.Token);

            // Ensure ReleaseHold is called whether the task completes or is canceled
            _executeTask.ContinueWith(task =>
            {
                ReleaseHold();
            }, TaskContinuationOptions.ExecuteSynchronously);

            return _executeTask ?? Task.CompletedTask; // Handle potential null value in _executeTask
        }

        /// <summary>
        /// Stops the service. This method should cancel the initialization task and ensure the service is properly cleaned up.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The token should request the service to stop.</param>
        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_executeTask == null || _stoppingCts == null)
            {
                return;
            }

            try
            {
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executeTask, Task.Delay(-1, cancellationToken));
                ReleaseHold(); // Ensure release is called even if stopping
            }
        }

        /// <summary>
        /// Releases all resources used by the service. This method should be called to clean up any resources when the service is no longer needed.
        /// </summary>
        public virtual void Dispose()
        {
            _stoppingCts?.Cancel();
            _stoppingCts?.Dispose();
        }
    }

    /// <summary>
    /// Provides an abstract base class for services that must wait for the completion of an initialization process before starting their operations.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Example usage:
    /// <code>
    /// services.AddHostedService&lt;MyStartup&gt;();
    /// services.AddHostedService&lt;MyDependend&gt;();
    /// 
    /// public class MyStartup : InitializationService
    /// {
    ///    private readonly ILogger&lt;MyStartup&gt; _logger;
    ///    public MyStartup(ILogger&lt;MyStartup&gt; logger) { _logger = logger; }
    ///
    ///    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///    {
    ///       _logger.LogInformation("Start MyStartup");
    ///       try { await Task.Delay(5000, stoppingToken); }
    ///       catch (TaskCanceledException) { _logger.LogInformation("MyStartup was canceled."); return; }
    ///       _logger.LogInformation("End MyStartup");
    ///    }
    /// }
    /// 
    /// public class MyDependend : InitializationDependentServices
    /// {
    ///    private readonly ILogger&lt;MyDependend&gt; _logger;
    ///    public MyDependend(ILogger&lt;MyDependend&gt; logger) { _logger = logger; }
    ///
    ///    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    ///    {
    ///       _logger.LogInformation("Start MyDependend");
    ///       try { await Task.Delay(5000, stoppingToken); }
    ///       catch (TaskCanceledException) { _logger.LogInformation("MyDependend was canceled."); return; }
    ///       _logger.LogInformation("End MyDependend");
    ///    }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public abstract class InitializationDependentServices : IHostedService, IDisposable
    {
        private Task? _executeTask;  // Made nullable as it may not be initialized immediately.
        private CancellationTokenSource? _stoppingCts; // Made nullable to reflect its lifecycle.

        /// <summary>
        /// When overridden in a derived class, performs the operational tasks once the service is started.
        /// This method is intended to contain the core logic for the dependent service, which should execute only after the initialization is complete.
        /// </summary>
        /// <param name="stoppingToken">A cancellation token that can be used to observe cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        protected abstract Task ExecuteAsync(CancellationToken stoppingToken);

        /// <summary>
        /// Starts the service asynchronously, waiting for an initialization signal before beginning execution.
        /// This method ensures that the service does not proceed until the initialization process has been completed and signaled accordingly.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests, which might come from the host application.</param>
        /// <returns>A task that represents the asynchronous operation of starting the service.</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Wait for the startup service to release.
            InitializationService.WaitForRelease();

            // Create a linked token source to manage cancellation more comprehensively.
            _stoppingCts = new CancellationTokenSource();
            CancellationToken linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stoppingCts.Token).Token;

            // Check if cancellation is already requested before starting the actual execution.
            if (linkedToken.IsCancellationRequested)
            {
                // Optionally, handle any pre-cancellation logic if necessary.
                return;
            }

            // Start the execution only if cancellation has not been requested.
            _executeTask = ExecuteAsync(linkedToken);

            // Optionally wait here if your service should wait for the task to complete in StartAsync or just fire and forget.
            await Task.CompletedTask;
        }

        /// <summary>
        /// Stops the service asynchronously. This method cancels the execution task and awaits its completion, ensuring a clean shutdown.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests, allowing for a graceful service stop.</param>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_stoppingCts != null)
            {
                _stoppingCts.Cancel();
            }

            if (_executeTask != null)
            {
                await Task.WhenAny(_executeTask, Task.Delay(-1, cancellationToken));
            }
        }

        /// <summary>
        /// Disposes of the resources used by the service, specifically the cancellation token source.
        /// </summary>
        public void Dispose()
        {
            _stoppingCts?.Dispose();
        }
    }
}