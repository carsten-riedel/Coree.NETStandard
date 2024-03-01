using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using Coree.NETStandard.Options;

namespace Coree.NETStandard.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHostedServices<T>(this IServiceCollection services) where T : class, IHostedService
        {
            return services.AddSingleton<IHostedService, T>();
        }

        public static IServiceCollection AddHostedServicesWithOptions<T, K>(this IServiceCollection services, K[] values) where K : class, new() where T : class, IHostedService
        {
            var optionsProvider = new HostedServicesWithOptionsProvider<K>();
            foreach (var value in values)
            {
                optionsProvider.Enqueue(value);
            }
            services.AddSingleton<IHostedServicesWithOptionsProvider<K>>(optionsProvider);

            for (int i = 0; i < values.Length; i++)
            {
                services.AddSingleton<IHostedService, T>();
            }

            return services;
        }

        public static IServiceCollection AddHostedServicesWithOptions<T, K>(this IServiceCollection services, IConfiguration configuration, string sectionName) where K : class, new() where T : class, IHostedService
        {
            var optionsProvider = new HostedServicesWithOptionsProvider<K>();
            services.AddSingleton<IHostedServicesWithOptionsProvider<K>>(optionsProvider);

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
