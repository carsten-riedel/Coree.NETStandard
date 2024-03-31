using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coree.NETStandard.HostedServicesCollection
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to facilitate the registration
    /// of hosted services with specific configurations.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {

        /// <summary>
        /// Registers a collection of hosted services of type <typeparamref name="T"/>
        /// with their configurations specified in a list of <typeparamref name="K"/>.
        /// </summary>
        /// <typeparam name="T">The type of the hosted service to register. Must implement <see cref="IHostedService"/>.</typeparam>
        /// <typeparam name="K">The configuration type for the hosted service. Must be a class with a parameterless constructor.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="values">A list of configuration instances for the hosted services.</param>
        /// <returns>The original <see cref="IServiceCollection"/> instance with the hosted services registered.</returns>
        /// <remarks>
        /// <code>
        /// services.AddHostedServicesCollection&lt;MyBackgroundSrv, MyBackgroundSrvConfig&gt;(new List&lt;MyBackgroundSrvConfig&gt;() {
        ///     new() { Name = "Service1"},
        ///     new() { Name = "Service2" }
        /// });
        ///
        /// public class MyBackgroundSrvConfig
        /// {
        ///     public Guid Guid { get; } = Guid.NewGuid();
        ///     public string? Name { get; set; }
        /// }
        ///
        /// public class MyBackgroundSrv : BackgroundService
        /// {
        ///     private readonly MyBackgroundSrvConfig options;
        ///     private readonly ILogger&lt;MyBackgroundService&gt; logger;
        ///
        ///     public MyBackgroundSrv(ILogger&lt;MyBackgroundService&gt; logger, IHostedServicesCollectionConfig&lt;MyBackgroundSrvConfig&gt; options)
        ///     {
        ///         this.logger = logger; this.options = options.FetchNextConfig();
        ///     }
        ///
        ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        ///     {
        ///         while (!stoppingToken.IsCancellationRequested)
        ///         {
        ///             try
        ///             {
        ///                 logger.LogInformation("MyBackgroundSrv running: {Name} with {guid} at {time}", options.Name, options.Guid.ToString(), DateTimeOffset.Now);
        ///                 await Task.Delay(5000, stoppingToken);
        ///             }
        ///             catch (TaskCanceledException)
        ///             {
        ///                 logger.LogInformation("MyBackgroundSrv cancel: {Name} with {guid} at {time}", options.Name, options.Guid.ToString(), DateTimeOffset.Now);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static IServiceCollection AddHostedServicesCollection<T, K>(this IServiceCollection services, List<K> values) where K : class, new() where T : class, IHostedService
        {
            return services.AddHostedServicesCollection<T, K>(values.ToArray());
        }

        /// <summary>
        /// Registers a collection of hosted services of type <typeparamref name="T"/>
        /// with their configurations specified in an array of <typeparamref name="K"/>.
        /// </summary>
        /// <typeparam name="T">The type of the hosted service to register. Must implement <see cref="IHostedService"/>.</typeparam>
        /// <typeparam name="K">The configuration type for the hosted service. Must be a class with a parameterless constructor.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="values">An array of configuration instances for the hosted services.</param>
        /// <returns>The original <see cref="IServiceCollection"/> instance with the hosted services registered.</returns>
        /// <remarks>
        /// Each hosted service instance is registered with a singleton service descriptor.
        /// <code>
        /// services.AddHostedServicesCollection&lt;MyBackgroundSrv, MyBackgroundSrvConfig&gt;(new MyBackgroundSrvConfig[] {
        ///     new() { Name = "Service1"},
        ///     new() { Name = "Service2" }
        /// });
        /// public class MyBackgroundSrvConfig
        /// {
        ///     public Guid Guid { get; } = Guid.NewGuid();
        ///     public string? Name { get; set; }
        /// }
        ///
        /// public class MyBackgroundSrv : BackgroundService
        /// {
        ///     private readonly MyBackgroundSrvConfig options;
        ///     private readonly ILogger&lt;MyBackgroundService&gt; logger;
        ///
        ///     public MyBackgroundSrv(ILogger&lt;MyBackgroundService&gt; logger, IHostedServicesCollectionConfig&lt;MyBackgroundSrvConfig&gt; options)
        ///     {
        ///         this.logger = logger; this.options = options.FetchNextConfig();
        ///     }
        ///
        ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        ///     {
        ///         while (!stoppingToken.IsCancellationRequested)
        ///         {
        ///             try
        ///             {
        ///                 logger.LogInformation("MyBackgroundSrv running: {Name} with {guid} at {time}", options.Name, options.Guid.ToString(), DateTimeOffset.Now);
        ///                 await Task.Delay(5000, stoppingToken);
        ///             }
        ///             catch (TaskCanceledException)
        ///             {
        ///                 logger.LogInformation("MyBackgroundSrv cancel: {Name} with {guid} at {time}", options.Name, options.Guid.ToString(), DateTimeOffset.Now);
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static IServiceCollection AddHostedServicesCollection<T, K>(this IServiceCollection services, K[] values) where K : class, new() where T : class, IHostedService
        {
            var optionsProvider = new HostedServicesCollectionConfig<K>();
            foreach (var value in values)
            {
                optionsProvider.Enqueue(value);
            }

            services.AddSingleton<IHostedServicesCollectionConfig<K>>(optionsProvider);

            for (int i = 0; i < values.Length; i++)
            {
                services.AddSingleton<IHostedService,T>();
            }

            return services;
        }

        /// <summary>
        /// Registers a collection of hosted services of type <typeparamref name="T"/>
        /// with their configurations loaded from a specified section of the application's <see cref="IConfiguration"/>.
        /// </summary>
        /// <typeparam name="T">The type of the hosted service to register. Must implement <see cref="IHostedService"/>.</typeparam>
        /// <typeparam name="K">The configuration type for the hosted service. Must be a class with a parameterless constructor.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
        /// <param name="configuration">The application's configuration.</param>
        /// <param name="sectionName">The name of the configuration section from which to load the configurations.</param>
        /// <returns>The original <see cref="IServiceCollection"/> instance with the hosted services registered.</returns>
        /// <remarks>
        /// Configurations are bound to new instances of <typeparamref name="K"/> and each hosted service
        /// instance is registered with a singleton service descriptor.
        /// <code>
        /// //appsettings.json
        /// "ServiceConfigurations": {
        ///     "FooService": {
        ///       "Name": "Service1"
        ///     },
        ///     "BarService": {
        ///       "Name": "Service2"
        ///     }
        /// }
        /// 
        /// services.AddHostedServicesCollection&lt;MyBackgroundSrv, MyBackgroundSrvConfig&gt;(context.Configuration, "ServiceConfigurations");
        ///
        /// public class MyBackgroundSrvConfig
        /// {
        ///     public Guid Guid { get; } = Guid.NewGuid();  public string? Name { get; set; }
        /// }
        ///
        /// public class MyBackgroundSrv : BackgroundService
        /// {
        ///     private readonly MyBackgroundSrvConfig options; private readonly ILogger&lt;MyBackgroundService&gt; logger;
        ///
        ///     public MyBackgroundSrv(ILogger&lt;MyBackgroundService&gt; logger, IHostedServicesCollectionConfig&lt;MyBackgroundSrvConfig&gt; options)
        ///     {
        ///         this.logger = logger; this.options = options.FetchNextConfig();
        ///     }
        ///
        ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        ///     {
        ///         while (!stoppingToken.IsCancellationRequested)
        ///         {
        ///             try {
        ///                 logger.LogInformation("MyBackgroundSrv running: {Name} with {guid} at {time}", options.Name, options.Guid.ToString(), DateTimeOffset.Now);
        ///                 await Task.Delay(5000, stoppingToken);
        ///             }
        ///             catch (TaskCanceledException)
        ///             { logger.LogInformation("MyBackgroundSrv cancel: {Name} with {guid} at {time}", options.Name, options.Guid.ToString(), DateTimeOffset.Now); }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </remarks>
        public static IServiceCollection AddHostedServicesCollection<T, K>(this IServiceCollection services, IConfiguration configuration, string sectionName) where K : class, new() where T : class, IHostedService
        {
            var optionsProvider = new HostedServicesCollectionConfig<K>();
            services.AddSingleton<IHostedServicesCollectionConfig<K>>(optionsProvider);

            var configSection = configuration.GetSection(sectionName);
            foreach (var childSection in configSection.GetChildren())
            {
                var configInstance = new K();
                childSection.Bind(configInstance);
                optionsProvider.Enqueue(configInstance);
            }

            for (int i = 0; i < configSection.GetChildren().Count(); i++)
            {
                services.AddSingleton<IHostedService, T>();
            }

            return services;
        }
    }
}