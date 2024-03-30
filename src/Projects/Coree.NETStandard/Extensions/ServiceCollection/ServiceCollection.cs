
using Coree.NETStandard.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;

namespace Coree.NETStandard.Extensions.ServiceCollection
{
    public static partial class ServiceCollectionExtensions
    {
        /*
            builder.ConfigureServices((context, services) =>
            {
                services.AddHostedServicesWithOptions<SingletonHostedServiceManager, SingletonHostedServiceManagerOptions>(new[] { new SingletonHostedServiceManagerOptions() {  Name = "service1" } , new SingletonHostedServiceManagerOptions() { Name = "service2" } });
                services.AddHostedServicesWithOptions<SingletonHostedServiceManager, SingletonHostedServiceManagerOptions>(context.Configuration, "ServiceConfigurations");
            });
         

        public static IServiceCollection AddHostedServicesWithOptions<T, K>(this IServiceCollection services, List<K> values) where K : class, new() where T : class, IHostedService
        {
            Log.Logger.ForSourceContext(nameof(AddHostedServicesWithOptions)).Verbose("aaaaa");
            return AddHostedServicesWithOptions<T, K>(services, values.ToArray());
        }
        */

        /// <summary>
        /// Adds a collection of hosted services to the specified IServiceCollection. Each hosted service
        /// type <typeparamref name="T"/> is configured with an instance of type <typeparamref name="K"/>.
        /// </summary>
        /// <typeparam name="T">The type of the hosted service. This type must implement IHostedService.</typeparam>
        /// <typeparam name="K">The configuration type for the hosted service. This type must be a class and have a parameterless constructor.</typeparam>
        /// <param name="services">The IServiceCollection to add the hosted services to.</param>
        /// <param name="values">A list of configuration values of type <typeparamref name="K"/> for the hosted services.</param>
        /// <returns>The original IServiceCollection instance, with the hosted services added.</returns>
        public static IServiceCollection AddHostedServicesCollection<T, K>(this IServiceCollection services, List<K> values) where K : class, new() where T : class, IHostedService
        {
            return services.AddHostedServicesCollection<T, K>(values.ToArray());
        }

        public static IServiceCollection AddHostedServicesCollection<T, K>(this IServiceCollection services, K[] values) where K : class, new() where T : class, IHostedService
        {
            var optionsProvider = new HostedServicesCollectionOptionsProvider<K>();
            foreach (var value in values)
            {
                optionsProvider.Enqueue(value);
            }

            services.AddSingleton<IHostedServicesCollectionOptionsProvider<K>>(optionsProvider);

            for (int i = 0; i < values.Length; i++)
            {
                services.AddSingleton<IHostedService, T>();
            }

            return services;
        }

        public static IServiceCollection AddHostedServicesCollection<T, K>(this IServiceCollection services, IConfiguration configuration, string sectionName) where K : class, new() where T : class, IHostedService
        {
            var optionsProvider = new HostedServicesCollectionOptionsProvider<K>();
            services.AddSingleton<IHostedServicesCollectionOptionsProvider<K>>(optionsProvider);

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
