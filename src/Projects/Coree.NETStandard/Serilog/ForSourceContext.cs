using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;

using SerilogCore = Serilog;

namespace Coree.NETStandard.Serilog
{
    /// <summary>
    /// Provides extension methods for Serilog ILogger instances to enhance logging capabilities.
    /// </summary>
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures the logger to associate a specified source context with log events.
        /// </summary>
        /// <param name="logger">The Serilog ILogger instance to configure.</param>
        /// <param name="value">The value to set for the SourceContext property in the log context.</param>
        /// <returns>A new ILogger instance that includes the specified source context.</returns>
        /// <example>
        /// Here is how you can use the <c>ForSourceContext</c> method:
        /// <code>
        /// var logger = Log.Logger;
        /// var contextualLogger = logger.ForSourceContext("MyApplication.Component");
        /// contextualLogger.Information("This is an informational message with a source context.");
        /// </code>
        /// </example>
        public static SerilogCore.ILogger ForSourceContext(this SerilogCore.ILogger logger, string value)
        {
            return logger.ForContext("SourceContext", value);
        }

        /// <summary>
        /// Configures the logger to associate the calling method's name with log events as the source context.
        /// </summary>
        /// <param name="logger">The Serilog ILogger instance to configure.</param>
        /// <returns>A new ILogger instance that includes the source context set to the calling method's name.</returns>
        public static SerilogCore.ILogger ForMethodContext(this SerilogCore.ILogger logger)
        {
            // Get the calling method name from the stack trace
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrame(1); // 1 to skip the current method frame
            var method = frame?.GetMethod();
            return logger.ForContext("SourceContext", method?.Name);
        }
    }
}
