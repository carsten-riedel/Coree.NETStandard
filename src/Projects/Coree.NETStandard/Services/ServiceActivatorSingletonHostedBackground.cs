using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Coree.Hosting.NETStandard.Services
{
    public class ServiceActivatorSingletonHostedBackground : IHostedService
    {
        private class KeyServices
        {
            public object? ServiceKey { get; set; }
            public Type? ServiceType { get; set; }
        }

        private readonly IServiceProvider serviceProvider;
        private readonly List<Type> UniqueServiceTypes;
        private readonly List<KeyServices> KeyedServices;

        public ServiceActivatorSingletonHostedBackground(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            var srv = serviceProvider.GetRegisteredServices();

            KeyedServices = srv
    .Where(descriptor => descriptor.IsKeyedService && typeof(IHostedService).IsAssignableFrom(descriptor.ServiceType))
    .Select(descriptor => new KeyServices // Use KeyServices instead of an anonymous type
    {
        ServiceKey = descriptor.ServiceKey,
        ServiceType = descriptor.ServiceType
    }) // Select both items as an anonymous type
    .Distinct() // This might need a custom equality comparer if you want to ensure uniqueness based on both properties
    .ToList();


            UniqueServiceTypes = srv
.Where(descriptor => descriptor.Lifetime == ServiceLifetime.Singleton // Singleton services
                     && typeof(IHostedService).IsAssignableFrom(descriptor.ServiceType) // Implements IHostedService
                     && !descriptor.ServiceType.Name.StartsWith("IHostedService")) // Not in the Microsoft namespace
.Select(descriptor => descriptor.ServiceType) // Select the service types
.Distinct() // Ensure distinct types
.ToList();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            foreach (var item in UniqueServiceTypes)
            {
                
                var services = serviceProvider.GetServices(item);
                foreach (var service in services)
                {
                    await ((IHostedService)service).StartAsync(cancellationToken);
                }
            }

            foreach (var item in KeyedServices)
            {
                var service = serviceProvider.GetRequiredKeyedService(item.ServiceType, item.ServiceKey);
                await ((IHostedService)service).StartAsync(cancellationToken);
            }


        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var item in UniqueServiceTypes)
            {
                var ss = serviceProvider.GetServices(item);
                foreach (var itemx in ss)
                {
                    await ((IHostedService)itemx).StopAsync(cancellationToken);
                }
            }

            foreach (var item in KeyedServices)
            {
                var service = serviceProvider.GetRequiredKeyedService(item.ServiceType, item.ServiceKey);
                await ((IHostedService)service).StopAsync(cancellationToken);
            }
        }
    }
}
