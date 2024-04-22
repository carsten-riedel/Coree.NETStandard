using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
    public abstract class ServiceFactory<T> : IDisposable where T : class
    {
        private static IHostBuilder? _hostBuilder;
        private static readonly IHost? _host;

        public static T CreateServiceFactory(Action<IHostBuilder>? configureHost = null)
        {
            return CreateServiceStack(null, null, configureHost);
        }

        public static T CreateServiceFactory(Action<IServiceCollection>? services = null)
        {
            return CreateServiceStack(services, null, null);
        }

        public static T CreateServiceFactory(Action<IServiceCollection>? configureServices = null, Action<ILoggingBuilder>? configureLogging = null, Action<IHostBuilder>? configureHost = null)
        {
            return CreateServiceStack(configureServices, configureLogging, configureHost);
        }

        public static T CreateServiceFactory()
        {
            return CreateServiceFactory(LogLevel.Trace);
        }

        public static T CreateServiceFactory(LogLevel logLevel)
        {
            return CreateServiceStack(configureServices: null, configureLogging: (log) => ConfigureDefaultLogging(log, logLevel), configureHost: (host) => ConfigureDefaultHost(host));
        }

        private static void ConfigureDefaultLogging(ILoggingBuilder log, LogLevel logLevel)
        {
            log.SetMinimumLevel(logLevel);
            log.AddFilter((category, level) => level >= logLevel);
        }

        private static void ConfigureDefaultHost(IHostBuilder host)
        {
            host.UseConsoleLifetime(lifeTimeOptions => { lifeTimeOptions.SuppressStatusMessages = true; });
        }

        /// <summary>
        /// Creates a new instance of the singleton class with logger and configuration services,
        /// resolving dependencies using dependency injection if available.
        /// </summary>
        /// <returns>A new instance of the singleton class.</returns>
        private static T CreateServiceStack(Action<IServiceCollection>? configureServices = null, Action<ILoggingBuilder>? configureLogging = null, Action<IHostBuilder>? configureHost = null)
        {
            try
            {
                _hostBuilder = Host.CreateDefaultBuilder();

                _hostBuilder.ConfigureServices(services =>
                {
                    services.AddLogging(loggingBuilder =>
                    {
                        configureLogging?.Invoke(loggingBuilder);
                    });
                    configureServices?.Invoke(services);
                });

                configureHost?.Invoke(_hostBuilder);

                IHost? app = _hostBuilder.Build();

                _host?.Start();

                var serviceProvider = app.Services.GetRequiredService<IServiceProvider>();

                T instance = ActivatorUtilities.CreateInstance<T>(serviceProvider);

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

        public void Dispose()
        {
            _host?.StopAsync().ConfigureAwait(false);
        }
    }
}