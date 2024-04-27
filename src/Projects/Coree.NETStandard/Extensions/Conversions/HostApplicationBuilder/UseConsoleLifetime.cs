using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace Coree.NETStandard.Extensions.Conversions.HostApplicationBuilder
{
    /// <summary>
    /// Provides extension methods for <see cref="IHostApplicationBuilder"/> to configure services, logging, app configuration, and host options in a manner similar to Host.CreateDefaultBuilder
    /// These extensions offer a fluent interface for configuring the application host, enabling the customization of service dependencies, configuration sources, logging, and host behaviors.
    /// </summary>
    public static partial class ConversionsHostApplicationBuilderExtensions
    {
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