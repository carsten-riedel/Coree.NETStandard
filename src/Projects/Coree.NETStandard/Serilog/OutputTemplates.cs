namespace Coree.NETStandard.Serilog
{
    /// <summary>
    /// Defines output templates for Serilog logging.
    /// </summary>
    public static class OutputTemplates
    {
        /// <summary>
        /// Provides the standard output template.
        /// </summary>
        /// <remarks>
        /// The returned template string is "{Timestamp:HH:mm:ss.ffff} {Level:u3} | {SourceContext} | {EnvironmentUserName} | {EnvironmentName} | {Message:l}{NewLine}{Exception}".
        /// </remarks>
        public static string Default()
        {
            return "{Timestamp:HH:mm:ss.ffff} {Level:u3} | {SourceContext} | {EnvironmentUserName} | {EnvironmentName} | {Message:l}{NewLine}{Exception}";
        }

        /// <summary>
        /// Provides a shortened version of the output template.
        /// </summary>
        /// <remarks>
        /// The returned template string is "{Timestamp:HH:mm:ss.ffff} | {Level:u3} | {SourceContextShort} | {Message:l}{NewLine}{Exception}".
        /// </remarks>
        public static string DefaultShort()
        {
            return "{Timestamp:HH:mm:ss.ffff} | {Level:u3} | {SourceContextShort} | {Message:l}{NewLine}{Exception}";
        }

        /// <summary>
        /// Returns a string template for logging with timestamp, log level, context, message, and exception details.
        /// </summary>
        /// <remarks>
        /// This method provides a template for logging messages with a structured format including timestamp,
        /// log level, context/source, message content, and exception details if present.
        /// </remarks>
        /// <returns>
        /// A string template that can be used for logging messages.
        /// </returns>
        public static string TwoLineShort()
        {
            return "{Timestamp:HH:mm:ss.ff} {Level:u3} Ctx: {SourceContext}{NewLine}{Timestamp:HH:mm:ss.ff} {Level:u3} Msg: {Message:l}{NewLine}{Exception}";
        }
    }
}