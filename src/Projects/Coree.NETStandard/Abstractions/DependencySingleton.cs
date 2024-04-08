using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    public abstract class DependencySingleton<T> : IDependencySingleton
    {
        private static readonly Lazy<T> instance = new Lazy<T>(CreateInstance);
        private static readonly IServiceCollection services = new ServiceCollection();
        private static IServiceProvider? serviceProvider;

        private static LogLevel _logLevelFilter = LogLevel.Information;
        private static readonly object logLevelLock = new object();

        /// <summary>
        /// Private property for getting or setting the minimum log level in a thread-safe manner.
        /// This log level acts as a global filter across the DependencySingleton, determining the verbosity of the logging output.
        /// Default is LogLevel.Information
        /// </summary>
        private static LogLevel LogLevelFilter
        {
            get
            {
                lock (logLevelLock)
                {
                    return _logLevelFilter;
                }
            }
            set
            {
                lock (logLevelLock)
                {
                    _logLevelFilter = value;
                }
            }
        }

        /// <summary>
        /// Sets the minimum log level filter for the application. This level acts as a filter for the logs that are emitted.
        /// Logs below this level will not be emitted. Default is Information
        /// </summary>
        /// <param name="logLevel">The log level to set as the minimum threshold for logging.</param>
        public void SetLogLevelFilter(LogLevel logLevel = LogLevel.Information)
        {
            LogLevelFilter = logLevel;
        }

        /// <summary>
        /// Gets the singleton instance of the class, ensuring thread safety.
        /// </summary>
        /// <returns>
        /// The singleton instance of type <typeparamref name="T"/>, which is the specific implementation of the 
        /// <see cref="DependencySingleton{T}"/>. Type <typeparamref name="T"/> represents the derived singleton class that includes
        /// implementations for required services such as logging and configuration. The instance is initialized with these services
        /// upon the first request, adhering to the singleton pattern to ensure only one instance is created and shared throughout
        /// the application.
        /// </returns>
        public static T Instance => instance.Value;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencySingleton{T}"/> class
        /// with the specified logger and configuration services.
        /// </summary>
        /// <param name="logger">The logger service.</param>
        /// <param name="configuration">The configuration service.</param>
        protected DependencySingleton(ILogger<T> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// Creates a new instance of the singleton class with logger and configuration services,
        /// resolving dependencies using dependency injection if available.
        /// </summary>
        /// <returns>A new instance of the singleton class.</returns>
        private static T CreateInstance()
        {
            try
            {
                if (serviceProvider == null)
                {
                    services.AddLogging(builder =>
                    {
                        builder.AddConsole();
                        builder.AddDebug();
                        builder.SetMinimumLevel(LogLevel.Trace);
                        builder.AddFilter((category, level) => level >= LogLevelFilter);
                    });

                    var configuration = new ConfigurationBuilder();
                    configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    var configBuild = configuration.Build();
                    services.AddSingleton<IConfiguration>(configBuild);

                    // Build the service provider
                    serviceProvider = services.BuildServiceProvider();
                }

                // Resolve the required services
                var logger = serviceProvider.GetRequiredService<ILogger<T>>();
                var config = serviceProvider.GetRequiredService<IConfiguration>();

                var instance = (T?)Activator.CreateInstance(typeof(T), logger, config);
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