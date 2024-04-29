using System;

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
        /// Configures host options for the application host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureOptions">The delegate to configure host options with the host builder and host options.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureHostOptions(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, HostOptions> configureOptions)
        {
            Action<IHostApplicationBuilder, HostOptions> configureOptions2 = configureOptions;
            return hostBuilder.ConfigureServices(delegate (IHostApplicationBuilder context, IServiceCollection collection)
            {
                collection.Configure(delegate (HostOptions options)
                {
                    configureOptions2(context, options);
                });
            });
        }

        /// <summary>
        /// Configures host options for the application host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureOptions">The delegate to configure host options.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureHostOptions(this IHostApplicationBuilder hostBuilder, Action<HostOptions> configureOptions)
        {
            Action<HostOptions> configureOptions2 = configureOptions;
            return hostBuilder.ConfigureServices(delegate (IServiceCollection collection)
            {
                collection.Configure(configureOptions2);
            });
        }
    }
}