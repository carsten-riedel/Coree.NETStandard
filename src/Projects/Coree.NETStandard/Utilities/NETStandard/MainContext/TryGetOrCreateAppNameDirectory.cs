using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Coree.NETStandard.Utilities
{
    /// <summary>
    /// Provides utility methods for managing the application's execution context, typically used within the Program.Main entry point.
    /// </summary>
    public static partial class MainContext
    {
        /// <summary>
        /// Attempts to get or create the configuration directory.
        /// </summary>
        /// <param name="throwIfFails">Indicates whether to throw an exception if the operation fails.</param>
        /// <returns>The path to the configuration directory, or null if not found or created.</returns>
        /// <example>
        /// <code>
        /// var configDir = TryGetCreateConfigDir();
        /// </code>
        /// </example>
        public static DirectoryInfo? TryGetOrCreateAppNameDirectory(string[]? baseDirectories = null, bool throwIfFails = false)
        {
            var appName = TryGetAppName();
            if (string.IsNullOrEmpty(appName))
            {
                return null;
            }

            baseDirectories ??= new[]
            {
                AppContext.BaseDirectory,
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                System.IO.Path.GetTempPath()
            };

            foreach (var baseDirectory in baseDirectories)
            {
                var result = EnsureWriteableDirectoryExists(baseDirectory, appName, throwIfFails);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
