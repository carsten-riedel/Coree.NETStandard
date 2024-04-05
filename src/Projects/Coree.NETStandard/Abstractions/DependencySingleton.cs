using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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
    /// public partial class SampleService : DependencySingleton&lt;SampleService&gt;, ISampleService
    /// {
    ///     public SampleService(ILogger&lt;SampleService&gt; logger, IConfiguration configuration) : base(logger, configuration) { }
    /// }
    /// 
    /// //File SampleSerivce.cs
    /// public partial class SampleService : DependencySingleton&lt;SampleService&gt;, ISampleService
    /// {
    ///     public void StandardDelay() { StandardDelayAsync(CancellationToken.None).GetAwaiter().GetResult(); }
    ///     public async Task StandardDelayAsync(CancellationToken cancellationToken = default)  { await Task.Delay(5000, cancellationToken); }
    /// }
    /// 
    /// //File Program.cs
    /// static async Task Main(string[] args)
    /// {
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
    public abstract class DependencySingleton<T>
    {
        private static readonly Lazy<T> instance = new Lazy<T>(CreateInstance);
        private static readonly IServiceCollection services = new ServiceCollection();
        private static IServiceProvider? serviceProvider;

        /// <summary>
        /// Gets the singleton instance of the class, ensuring thread safety.
        /// </summary>
        public static T Instance => instance.Value;

        protected readonly ILogger<T> logger;
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
                    });

                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();
                    services.AddSingleton<IConfiguration>(configuration);

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
