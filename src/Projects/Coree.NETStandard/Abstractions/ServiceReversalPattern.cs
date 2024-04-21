using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static System.Collections.Specialized.BitVector32;

namespace Coree.NETStandard.Abstractions
{
    /// <summary>
    /// Represents an abstract base class for creating thread-safe singleton instances with optional dependency injection support.
    /// This pattern can be utilized in both dependency injection (DI) and non-DI scenarios.
    /// Example usage:
    /// <code>
    ///
    /// //File ISampleSerivce.cs
    /// public interface ISampleService
    /// {
    ///     Task StandardDelayAsync(CancellationToken cancellationToken = default);  void StandardDelay();
    /// }
    ///
    /// //File DSSampleSerivce.cs
    /// public partial class SampleService : DependencySingleton&lt;SampleService&gt;, ISampleService , IDependencySingleton
    /// {
    ///     public SampleService(ILogger&lt;SampleService&gt; logger, IConfiguration configuration) : base(logger, configuration) { }
    /// }
    ///
    /// //File SampleSerivce.cs
    /// public partial class SampleService : DependencySingleton&lt;SampleService&gt;, ISampleService , IDependencySingleton
    /// {
    ///     public void StandardDelay() { StandardDelayAsync(CancellationToken.None).GetAwaiter().GetResult(); }
    ///     public async Task StandardDelayAsync(CancellationToken cancellationToken = default)  { await Task.Delay(5000, cancellationToken); }
    /// }
    ///
    /// //File Program.cs
    /// static async Task Main(string[] args)
    /// {
    ///    SampleService.Instance.SetMinimumLogLevel(LogLevel.Trace);
    ///    // The ILogger&lt;SampleService&gt; logger and IConfiguration configuration will be initialized with their own service stacks.
    ///    SampleService.Instance.StandardDelay();
    ///
    ///    // Normal DI usage
    ///    var host = Host.CreateDefaultBuilder(args)
    ///        .ConfigureLogging((context, logging) => {  logging.AddConsole(); logging.AddDebug(); })
    ///        .ConfigureServices((context, services) => { services.AddSingleton&lt;ISampleService,SampleService&gt;(); services.AddSingleton&lt;IMyService,MyService&gt;(); });
    ///    await host.Build().RunAsync();
    /// }
    ///
    /// //Service usage
    /// public class MyService
    /// {
    ///     private readonly ISampleService sampleService;
    ///
    ///     public MyService(ISampleService sampleService)
    ///     {
    ///         this.sampleService = sampleService;
    ///     }
    ///
    ///     public async Task UseSample()
    ///     {
    ///         await sampleService.StandardDelayAsync();
    ///     }
    ///}
    /// </code>
    /// </summary>
    /// <typeparam name="T">The type of the singleton instance.</typeparam>
    public abstract class ServiceReversalPattern<T>
    {
        private static IHostBuilder? host;

        private static LogLevel _logLevelFilter = LogLevel.Information;

        public static T GetControl(Action<IServiceCollection>? configureServices = null, Action<ILoggingBuilder>? configureLogging = null)
        {
            return CreateServiceStack(configureServices, configureLogging);
        }

        /// <summary>
        /// Provides logging capabilities for the singleton instance. By default, the logger's minimum logging level is set to Trace,
        /// allowing all log messages to be captured. However, the default filter level applied to log messages is set to Information,
        /// meaning that only logs at Information level or higher will be emitted unless otherwise adjusted. The logging level filter
        /// can be dynamically changed at runtime using the <see cref="SetLogLevelFilter"/> method to control the verbosity of the logging output.
        /// </summary>
        protected readonly ILogger<T> logger;

        /// <summary>
        /// Represents the configuration for the application. This encompasses settings from
        /// various configuration sources (e.g., appsettings.json, environment variables) and
        /// is used to access DependencySingleton settings such as connection strings, feature
        /// toggles, and other configurable aspects of the application.
        /// </summary>
        protected readonly IConfiguration configuration;

        protected readonly IHostEnvironment hostEnvironment;

        protected readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencySingleton{T}"/> class
        /// with the specified logger and configuration services.
        /// </summary>
        /// <param name="logger">The logger service.</param>
        /// <param name="configuration">The configuration service.</param>
        protected ServiceReversalPattern(ILogger<T> logger, IConfiguration configuration, IHostEnvironment hostEnvironment, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.hostEnvironment = hostEnvironment;
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates a new instance of the singleton class with logger and configuration services,
        /// resolving dependencies using dependency injection if available.
        /// </summary>
        /// <returns>A new instance of the singleton class.</returns>
        private static T CreateServiceStack(Action<IServiceCollection>? configureServices = null, Action<ILoggingBuilder>? configureLogging = null)
        {
            try
            {
                host = Host.CreateDefaultBuilder();
                host.ConfigureServices(services =>
                {
                    services.AddLogging(loggingBuilder =>
                    {
                        if (configureLogging != null)
                        {
                            configureLogging?.Invoke(loggingBuilder);
                        }
                        else
                        {
                            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
                            loggingBuilder.AddFilter((category, level) => level >= LogLevel.Trace);
                        }
                    });
                    configureServices?.Invoke(services);
                });

                var app = host.Build();

                var logger = app.Services.GetRequiredService<ILogger<T>>();
                var config = app.Services.GetRequiredService<IConfiguration>();
                var hostingEnvironment = app.Services.GetRequiredService<IHostEnvironment>();
                var serviceProvider = app.Services.GetRequiredService<IServiceProvider>();

                var instance = (T?)Activator.CreateInstance(typeof(T), logger, config, hostingEnvironment, serviceProvider);
                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance));
                }
                return instance;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create instance.", ex);
            }
        }
    }
}