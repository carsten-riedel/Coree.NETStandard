using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coree.NETStandard.Extensions.Conversions.HostApplicationBuilder
{
    /// <summary>
    /// Provides extension methods for <see cref="IHostApplicationBuilder"/> to configure services, logging, app configuration, and host options in a manner similar to Host.CreateDefaultBuilder
    /// These extensions offer a fluent interface for configuring the application host, enabling the customization of service dependencies, configuration sources, logging, and host behaviors.
    /// </summary>
    public static partial class ConversionsHostApplicationBuilderExtensions
    {
        /// <summary>
        /// Configures services for the application host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureDelegate">The delegate to configure services with the host builder and service collection.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureServices(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, IServiceCollection> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder, hostBuilder.Services);
            return hostBuilder;
        }

        /// <summary>
        /// Configures services for the application host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureDelegate">The delegate to configure services with the service collection.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureServices(this IHostApplicationBuilder hostBuilder, Action<IServiceCollection> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder.Services);
            return hostBuilder;
        }

    }
}
