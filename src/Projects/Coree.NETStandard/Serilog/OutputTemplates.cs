using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
