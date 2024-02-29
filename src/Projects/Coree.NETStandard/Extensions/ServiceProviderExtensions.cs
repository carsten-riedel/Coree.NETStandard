using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Coree.NETStandard
{
    public static class ServiceProviderExtensions
    {
        public static List<ServiceDescriptor> GetRegisteredServices(this IServiceProvider serviceProvider)
        {
            List<ServiceDescriptor> serviceDescriptors = new List<ServiceDescriptor>();
            // Attempt to access the internal service collection
            var serviceScopeFactory = serviceProvider.GetService<IServiceScopeFactory>();
            var scope = serviceScopeFactory.CreateScope();
            try
            {

                var ss = scope.ServiceProvider.GetType();

                var root = (ServiceProvider)scope.ServiceProvider.GetType().GetProperty("RootProvider", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(scope.ServiceProvider);
                var calls = root.GetType().GetProperty("CallSiteFactory", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(root);
                IEnumerable<ServiceDescriptor>? desc = (System.Collections.Generic.IEnumerable<Microsoft.Extensions.DependencyInjection.ServiceDescriptor>)calls.GetType().GetProperty("Descriptors", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(calls);
                serviceDescriptors.AddRange(desc);
            }
            finally
            {
                scope.Dispose();
            }

            return serviceDescriptors;
        }

    }
}
