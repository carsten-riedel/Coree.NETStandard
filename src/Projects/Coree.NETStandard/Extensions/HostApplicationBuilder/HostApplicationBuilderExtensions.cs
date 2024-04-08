using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="IHostApplicationBuilder"/> to configure services, logging, app configuration, and host options in a manner similar to Host.CreateDefaultBuilder
    /// These extensions offer a fluent interface for configuring the application host, enabling the customization of service dependencies, configuration sources, logging, and host behaviors.
    /// </summary>
    public static class HostApplicationBuilderExtensions
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

        /// <summary>
        /// Configures logging for the application host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureDelegate">The delegate to configure logging with the host builder and logging builder.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder hostBuilder, Action<IHostApplicationBuilder, ILoggingBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder, hostBuilder.Logging);
            return hostBuilder;
        }

        /// <summary>
        /// Configures logging for the application host builder using a delegate.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <param name="configureDelegate">The delegate to configure logging with the logging builder.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder hostBuilder, Action<ILoggingBuilder> configureDelegate)
        {
            configureDelegate.Invoke(hostBuilder.Logging);
            return hostBuilder;
        }

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

        /// <summary>
        /// Configures the application host builder to use a console lifetime.
        /// </summary>
        /// <param name="hostBuilder">The application host builder.</param>
        /// <returns>The <see cref="IHostApplicationBuilder"/> for chaining.</returns>
        public static IHostApplicationBuilder UseConsoleLifetime(this IHostApplicationBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(delegate (IServiceCollection collection)
            {
                collection.AddSingleton<IHostLifetime, ConsoleLifetime>();
            });
        }
    }
}
