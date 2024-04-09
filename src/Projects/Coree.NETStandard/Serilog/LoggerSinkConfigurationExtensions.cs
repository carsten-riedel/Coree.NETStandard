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

    /// <summary>
    /// Provides extension methods for configuring Serilog sinks with conditional log level processing.
    /// </summary>
    public static class LoggerSinkConfigurationExtensions
    {
        /// <summary>
        /// Adds a console sink with conditional log level processing to the logger configuration.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration to modify.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required for events to be passed to the sink.</param>
        /// <param name="outputTemplate">The output template determining the format of log events.</param>
        /// <param name="formatProvider">The format provider for formatting log event properties.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through log event level to be changed at runtime.</param>
        /// <param name="standardErrorFromLevel">The log event level at which to start writing to standard error instead of standard output.</param>
        /// <param name="theme">The theme to apply to the output.</param>
        /// <param name="applyThemeToRedirectedOutput">Whether to apply the theme to redirected output streams.</param>
        /// <param name="syncRoot">An object to synchronize the log event emission.</param>
        /// <param name="conditionalLevel">A dictionary mapping specific conditions to log event levels for conditional processing.</param>
        /// <returns>The modified logger configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when loggerConfiguration is null.</exception>
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

        /// <summary>
        /// Adds a debug sink with conditional log level processing to the logger configuration.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration to modify.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required for events to be passed to the sink.</param>
        /// <param name="outputTemplate">The output template determining the format of log events.</param>
        /// <param name="formatProvider">The format provider for formatting log event properties.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through log event level to be changed at runtime.</param>
        /// <param name="conditionalLevel">A dictionary mapping specific conditions to log event levels for conditional processing.</param>
        /// <returns>The modified logger configuration.</returns>
        /// <exception cref="ArgumentNullException">Thrown when loggerConfiguration is null.</exception>
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
