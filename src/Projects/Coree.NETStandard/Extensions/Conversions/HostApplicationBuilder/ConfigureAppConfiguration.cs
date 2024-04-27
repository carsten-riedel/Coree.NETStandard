using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
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
        /// Configures application configuration for the host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureDelegate">The delegate to configure the application configuration with the host builder and configuration builder.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureAppConfiguration(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, IConfigurationBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder, hostBuilder.Configuration);
            return hostBuilder;
        }

        /// <summary>
        /// Configures application configuration for the host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureDelegate">The delegate to configure the application configuration with the configuration builder.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureAppConfiguration(this IHostApplicationBuilder hostBuilder, Action<IConfigurationBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder.Configuration);
            return hostBuilder;
        }

    }
}
