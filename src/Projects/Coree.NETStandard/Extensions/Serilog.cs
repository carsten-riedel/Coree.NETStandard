using Coree.NETStandard.Logging;
using Serilog;
using Serilog.Configuration;
using Serilog.Filters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks.Sources;

namespace Coree.NETStandard.Extensions
{
    public static partial class SerilogExtensions
    {
        // Extension method for Serilog.ILogger for reinitialization with an optional action
        public static void UseConfigurationMethod(this Serilog.ILogger logger, Action<LoggerConfiguration>? action = null)
        {
            var loggerConfiguration = new LoggerConfiguration();
            loggerConfiguration.UseConfigurationMethod(action);
            Log.Logger = loggerConfiguration.CreateLogger();
        }

        // Extension method for LoggerConfiguration to apply configurations
        public static LoggerConfiguration UseConfigurationMethod(this LoggerConfiguration config, Action<LoggerConfiguration>? action = null)
        {
            // If action is not null, invoke it, otherwise apply default config
            if (action != null)
            {
                action.Invoke(config);
            }
            else
            {
                CommonConsoleConfig(config);
            }
            return config;
        }


        public static void CommonConsoleConfig(LoggerConfiguration config)
        {
            config
                .Enrich.FromLogContext()
                .Enrich.WithProcessId()
                .Enrich.With(new SourceContextShortEnricher(true,true,true))
                .Enrich.WithThreadId()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithEnvironmentName()
                .MinimumLevel.Verbose()
                .WriteTo.Logger(lc => lc.Filter.ByExcluding(Matching.FromSource("Microsoft.AspNetCore"))).WriteTo.Console(outputTemplate: CommonConsoleConfigOutputTemplate(), restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose);
        }

        public static string CommonConsoleConfigOutputTemplate()
        {
            return "{Timestamp:HH:mm:ss.ffff} {Level:u3}|{SourceContext}| {EnvironmentUserName} | {EnvironmentName} | {Message:l}{NewLine} {Exception}";
        }

        public static string SimpleOutputTemplate()
        {
            return "{Timestamp:HH:mm:ss.ffff} | {Level:u3} | {SourceContextShort} | {Message:l}{NewLine}{Exception}";
        }

        public static ILogger ForSourceContext(this ILogger logger,string? sourceContext)
        {
            return logger.ForContext("SourceContext", sourceContext);
        }
    }
}