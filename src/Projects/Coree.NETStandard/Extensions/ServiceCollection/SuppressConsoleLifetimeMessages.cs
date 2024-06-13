using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coree.NETStandard.Extensions.ServiceCollection
{
    /// <summary>
    /// Provides extension methods for <see cref="IServiceCollection"/> to configure application services.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures ConsoleLifetimeOptions to suppress status messages.
        /// </summary>
        /// <param name="services">The IServiceCollection to configure.</param>
        public static IServiceCollection SuppressConsoleLifetimeMessages(this IServiceCollection services)
        {
            services.Configure<ConsoleLifetimeOptions>(options =>
            {
                options.SuppressStatusMessages = true;
            });

            return services;
        }
    }
}
