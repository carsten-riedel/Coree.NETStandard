using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Coree.NETStandard.Serilog
{
    /// <summary>
    /// Extension methods for configuring logging services.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds and configures Serilog logging to the specified IServiceCollection, incorporating a conditional log level feature.
        /// </summary>
        /// <param name="services">The IServiceCollection to which logging services are added.</param>
        /// <param name="simplifyNamespace">If true, simplifies the namespace to only include the last component.</param>
        /// <param name="conditionalLevel">
        /// An optional dictionary mapping source contexts to LogEventLevels, allowing for dynamic log level overrides.
        /// Example usage:
        /// <code>
        /// new Dictionary&lt;string, LogEventLevel&gt;
        /// {
        ///     ["Coree.SomeService"] = LogEventLevel.Debug,
        /// };
        /// </code>
        /// This parameter enables conditional log level adjustment based on the source context, offering fine-grained control over log verbosity without direct reference to the source contexts at runtime.
        /// </param>
        /// <returns>The original IServiceCollection, supporting method chaining.</returns>
        /// <remarks>
        /// Configures Serilog as the primary logging provider, leveraging a LoggingLevelSwitch for default level control. It enriches log entries with contextual information and enables output to both console and debug sinks, using a concise output format. This setup ensures that logs are both informative and manageable, tailored to development and production environments.
        /// </remarks>
        public static IServiceCollection AddLoggingCoreeNETStandard(this IServiceCollection services,bool simplifyNamespace = true, Dictionary<string, LogEventLevel>? conditionalLevel = null)
        {
            LoggingLevelSwitch loggingLevelSwitch = new LoggingLevelSwitch();
            services.AddSingleton(loggingLevelSwitch);
            services.AddLogging(configure =>
            {
                configure.ClearProviders();

                var loggerConfig = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.With(new SourceContextShortEnricher(true, simplifyNamespace, 15, null))
                    .WriteTo.ConsoleConditionalLevel(outputTemplate: OutputTemplates.DefaultShort(), conditionalLevel: conditionalLevel)
                    .WriteTo.DebugConditionalLevel(outputTemplate: OutputTemplates.DefaultShort(), conditionalLevel: conditionalLevel);

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
