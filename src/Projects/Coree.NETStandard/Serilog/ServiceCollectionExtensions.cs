using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;

namespace Coree.NETStandard.Serilog
{
    /// <summary>
    /// Extension methods for configuring logging services.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures Serilog logging to the specified IServiceCollection.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <returns>The original IServiceCollection for chaining.</returns>
        /// <remarks>
        /// This method configures Serilog as the logging provider with a default logging level controlled by a LoggingLevelSwitch.
        /// It enriches logs with context and supports output to console and debug sinks with a short output format.
        /// </remarks>
        public static IServiceCollection AddLoggingCoreeNETStandard(this IServiceCollection services)
        {
            LoggingLevelSwitch loggingLevelSwitch = new LoggingLevelSwitch();
            services.AddSingleton(loggingLevelSwitch);
            services.AddLogging(configure =>
            {
                configure.ClearProviders();

                var loggerConfig = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.With(new SourceContextShortEnricher())
                    .WriteTo.Console(outputTemplate: OutputTemplates.DefaultShort())
                    .WriteTo.Debug(outputTemplate: OutputTemplates.DefaultShort());

                if (loggingLevelSwitch != null)
                {
                    loggerConfig.MinimumLevel.ControlledBy(loggingLevelSwitch);
                }

                configure.AddSerilog(loggerConfig.CreateLogger(), dispose: true);
            });

            return services;
        }
    }
}
