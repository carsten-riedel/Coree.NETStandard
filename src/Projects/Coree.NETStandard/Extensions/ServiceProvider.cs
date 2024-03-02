using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Coree.NETStandard.Extensions
{
    public static class ServiceProviderExtensions
    {
        

        public static List<ServiceDescriptor> GetRegisteredServices(this IServiceProvider serviceProvider)
        {
            List<ServiceDescriptor> serviceDescriptors = [];
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
