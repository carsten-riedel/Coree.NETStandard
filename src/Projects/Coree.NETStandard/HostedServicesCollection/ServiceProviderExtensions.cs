using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <summary>
        /// Provides a lazy-initialized collection of hosted services of the specified type <typeparamref name="T"/>,
        /// designed to be used in scenarios where direct service resolution in constructors may lead to race conditions.
        /// </summary>
        /// <remarks>
        /// This method is particularly useful in complex dependency scenarios, where some services may not be fully
        /// initialized at the time of a constructor's execution. By deferring the service resolution to the actual
        /// execution time of a method, it mitigates the risk of encountering race conditions within the service
        /// initialization stack. It leverages <see cref="Lazy{T}"/> to ensure that the service collection is resolved
        /// only when it is first needed, thereby enhancing reliability and performance by avoiding unnecessary upfront
        /// service resolution and instantiation.
        /// </remarks>
        /// <typeparam name="T">The type of the hosted service to retrieve. This type parameter is contravariant.</typeparam>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> from which to retrieve the services.</param>
        /// <returns>
        /// A <see cref="Lazy{T}"/> object that, when evaluated, provides an <see cref="IEnumerable{T}"/> representing
        /// the collection of services of the specified type <typeparamref name="T"/>. If no services of the specified
        /// type are registered, an empty enumerable is returned. This allows safe and deferred resolution of services,
        /// ideal for use within method implementations to avoid race conditions in service stacks.
        /// </returns>
        /// <example>
        /// This example demonstrates using <see cref="GetHostedServiceCollectionOfTypeLazy{T}"/> within a service constructor
        /// to defer the resolution of dependent services, mitigating race conditions:
        /// <code>
        /// public class MyService :BackgroundService
        /// {
        ///     private readonly Lazy&lt;IEnumerable&lt;MyDependentService&gt;&gt; _dependentServices;
        ///
        ///     public MyService(IServiceProvider serviceProvider)
        ///     {
        ///         _dependentServices = serviceProvider.GetHostedServiceCollectionOfTypeLazy&lt;MyDependentService&gt;();
        ///     }
        ///
        ///     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        ///     {
        ///         while (!stoppingToken.IsCancellationRequested)
        ///         {
        ///             // The dependent services are resolved here, when first needed, avoiding race conditions.
        ///             foreach(var service in _dependentServices.Value)
        ///             {
        ///                 // Interact with the service
        ///             }
        ///         }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static Lazy<IEnumerable<T>> GetHostedServiceCollectionOfTypeLazy<T>(this IServiceProvider serviceProvider)
        {
            return new Lazy<IEnumerable<T>>(() =>
            {
                var services = serviceProvider.GetService<IEnumerable<IHostedService>>() ?? Enumerable.Empty<IHostedService>();
                var ret = services.OfType<T>();
                return ret.Any() ? ret : Enumerable.Empty<T>();
            });
        }
    }
}