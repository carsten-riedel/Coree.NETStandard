using System;
using System.Collections.Generic;
using System.Text;

namespace Coree.NETStandard.Utilities
{
    /// <summary>
    /// Provides utility methods for managing the application's execution context, typically used within the Program.Main entry point.
    /// </summary>
    public static partial class MainContext
    {
        /// <summary>
        /// Tries to get the application name from various sources.
        /// </summary>
        /// <returns>The application name, or null if it cannot be determined.</returns>
        /// <example>
        /// <code>
        /// var appName = TryGetAppName();
        /// </code>
        /// </example>
        public static string? TryGetAppName()
        {
            return System.IO.Path.GetFileNameWithoutExtension(TryGetPrimaryFileLocation());
        }
    }
}
