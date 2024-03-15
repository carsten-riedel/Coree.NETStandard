using Coree.NETStandard.Logging;
using Coree.NETStandard.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Collections.Generic;
using System.Linq;

namespace Coree.NETStandard.Extensions.ServiceCollection
{
    public static class ServiceCollectionExtensions
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
