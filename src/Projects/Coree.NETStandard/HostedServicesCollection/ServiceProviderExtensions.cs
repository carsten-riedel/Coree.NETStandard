using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coree.NETStandard.HostedServicesCollection
{
    /// <summary>
    /// Provides a set of extension methods for <see cref="IServiceProvider"/>. These methods extend the functionality
    /// of <see cref="IServiceProvider"/> to offer enhanced and simplified service resolution capabilities,
    /// enabling the retrieval and management of service instances in a more flexible manner.
    /// </summary>
    public static partial class ServiceProviderExtensions
    {
        public static IEnumerable<T> GetHostedServiceCollection<T>(this IServiceProvider serviceProvider)
        {
            var ret = serviceProvider.GetServices<IHostedService>().OfType<T>();

            if (ret != null)
            {
                return ret;
            }

            throw new InvalidOperationException($"No services registered for implementation type {nameof(T)}.");
        }

  
        public static IEnumerable<T> GetActiveHostedServiceCollection<T>(this IServiceProvider serviceProvider) where T : class, IHostedService
        {
            // Retrieve all IHostedService instances
            var hostedServices = serviceProvider.GetServices<IHostedService>();

            // Filter out services based on the 'ExecuteTask' property being a non-completed Task
            var filteredServices = hostedServices
                .Where(service =>
                {
                    var executeTaskProperty = service.GetType().GetProperty("ExecuteTask", BindingFlags.Public | BindingFlags.Instance);
                    if (executeTaskProperty != null)
                    {
                        var executeTaskValue = executeTaskProperty.GetValue(service) as Task;
                        // Ensure the task is not null and has not completed
                        return executeTaskValue != null && !executeTaskValue.IsCompleted;
                    }
                    return false; // Exclude services without an 'ExecuteTask' property or with a null value
                })
                .OfType<T>();

            // Check if any services are found after filtering
            if (filteredServices.Any())
            {
                return filteredServices;
            }
            else
            {
                return Enumerable.Empty<T>();
            }
        }

    }
}