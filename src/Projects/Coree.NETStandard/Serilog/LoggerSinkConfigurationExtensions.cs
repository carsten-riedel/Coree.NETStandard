using System;
using System.Collections.Generic;
using System.Text;

using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;

namespace Coree.NETStandard.Serilog
{

    // Extension method to easily add the LevelOverrideSink with an existing console configuration
    public static class LoggerSinkConfigurationExtensions
    {
        public static LoggerConfiguration ConsoleConditionalLevel(this LoggerSinkConfiguration loggerConfiguration, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", IFormatProvider? formatProvider = null, LoggingLevelSwitch? levelSwitch = null, LogEventLevel? standardErrorFromLevel = null, ConsoleTheme? theme = null, bool applyThemeToRedirectedOutput = false, object? syncRoot = null, Dictionary<string, LogEventLevel>? conditionalLevel = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            // Configure the console sink here directly
            var consoleSink = new LoggerConfiguration().WriteTo.Console(
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                outputTemplate: outputTemplate,
                formatProvider: formatProvider,
                levelSwitch: levelSwitch,
                standardErrorFromLevel: standardErrorFromLevel,
                theme: theme,
                applyThemeToRedirectedOutput: applyThemeToRedirectedOutput,
                syncRoot: syncRoot
                ).CreateLogger();

            return loggerConfiguration.Sink(new ConditionalLevelSink(consoleSink, conditionalLevel));
        }

        public static LoggerConfiguration DebugConditionalLevel(this LoggerSinkConfiguration loggerConfiguration, LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose, string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", IFormatProvider? formatProvider = null, LoggingLevelSwitch? levelSwitch = null, Dictionary<string, LogEventLevel>? conditionalLevel = null)
        {
            if (loggerConfiguration == null) throw new ArgumentNullException(nameof(loggerConfiguration));
            // Configure the console sink here directly
            var debugSink = new LoggerConfiguration().WriteTo.Debug(
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                outputTemplate: outputTemplate,
                formatProvider: formatProvider,
                levelSwitch: levelSwitch
                ).CreateLogger();

            return loggerConfiguration.Sink(new ConditionalLevelSink(debugSink, conditionalLevel));
        }
    }
}
