using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Coree.NETStandard.Extensions
{
    /// <summary>
    /// Provides an extension method for <see cref="IServiceProvider"/> to retrieve all services currently registered.
    /// This functionality aids in accessing service-related information, useful for debugging or service analysis purposes.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Retrieves a list of all service descriptors registered in the service provider. This method utilizes reflection
        /// to access internal properties of the service provider, which could be subject to change in future .NET versions.
        /// </summary>
        /// <param name="serviceProvider">The service provider instance.</param>
        /// <returns>A list of <see cref="ServiceDescriptor"/> objects representing all registered services.</returns>
        /// <exception cref="Exception">Throws an exception if internal properties cannot be accessed.</exception>
        public static List<ServiceDescriptor> GetRegisteredServices(this IServiceProvider serviceProvider)
        {
            List<ServiceDescriptor> serviceDescriptors = new();
            // Attempt to access the internal service collection
            IServiceScopeFactory? serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            if (serviceScopeFactory is null) { return serviceDescriptors; }
            IServiceScope scope = serviceScopeFactory.CreateScope();
            try
            {
                ServiceProvider? rootProvider = (ServiceProvider)scope.ServiceProvider.GetType().GetProperty("RootProvider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(scope.ServiceProvider);
                object CallSiteFactory = rootProvider.GetType().GetProperty("CallSiteFactory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(rootProvider);
                IEnumerable<ServiceDescriptor>? desc = (IEnumerable<ServiceDescriptor>)CallSiteFactory.GetType().GetProperty("Descriptors", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(CallSiteFactory);
                serviceDescriptors.AddRange(desc);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                scope.Dispose();
            }

            return serviceDescriptors;
        }
    }
}