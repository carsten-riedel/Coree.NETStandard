using System;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Extensions.Conversions.HostApplicationBuilder
{
    /// <summary>
    /// Provides extension methods for <see cref="IHostApplicationBuilder"/> to configure services, logging, app configuration, and host options in a manner similar to Host.CreateDefaultBuilder
    /// These extensions offer a fluent interface for configuring the application host, enabling the customization of service dependencies, configuration sources, logging, and host behaviors.
    /// </summary>
    public static partial class ConversionsHostApplicationBuilderExtensions
    {
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
    }
}