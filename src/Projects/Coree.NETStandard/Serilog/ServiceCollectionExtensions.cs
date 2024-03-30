using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;

namespace Coree.NETStandard.Serilog
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLoggingCoreeNETStandard(this IServiceCollection services)
        {
            LoggingLevelSwitch loggingLevelSwitch = new LoggingLevelSwitch();
            services.AddSingleton(loggingLevelSwitch);
            services.AddLogging(configure =>
            {
                configure.ClearProviders();

                // Start configuring the Logger
                var loggerConfig = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .Enrich.With(new SourceContextShortEnricher()) // Ensure this enricher is defined in your project
                    .WriteTo.Console(outputTemplate: OutputTemplates.DefaultShort()) // Ensure this template method is defined
                    .WriteTo.Debug(outputTemplate: OutputTemplates.DefaultShort()); // And this one too

                // Conditionally apply the minimum logging level control
                if (loggingLevelSwitch != null)
                {
                    loggerConfig.MinimumLevel.ControlledBy(loggingLevelSwitch);
                }

                // Complete the configuration and add Serilog as the logging provider
                configure.AddSerilog(loggerConfig.CreateLogger(), dispose: true);
            });

            return services;
        }
    }
}