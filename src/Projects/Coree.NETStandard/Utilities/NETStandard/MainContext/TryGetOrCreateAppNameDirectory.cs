using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Coree.NETStandard.Extensions.Conversions.String;

namespace Coree.NETStandard.Utilities
{
    /// <summary>
    /// Provides utility methods for managing the application's execution context, typically used within the Program.Main entry point.
    /// </summary>
    public static partial class MainContext
    {
        /// <summary>
        /// Attempts to get or create the application name directory.
        /// <param name="baseDirectories">Optional array of base directories to search or create the application name directory in. If not provided, default directories will be used.</param>
        /// <param name="useStartupAppLocation">Indicates whether to use a startup-specific application location.</param>
        /// <param name="defaultDirectories">Optional array of default directories to ensure they exist within the application directory.</param>
        /// <param name="throwIfFails">Indicates whether to throw an exception if the operation fails.</param>
        /// <returns>The path to the application name directory, or null if not found or created.</returns>
        /// <example>
        /// <code>
        /// var configDir = MainContext.TryGetOrCreateAppNameDirectory();
        /// </code>
        /// </example>
        /// </summary>
        public static DirectoryInfo? TryGetOrCreateAppNameDirectory(string[]? baseDirectories = null,bool useStartupAppLocation = true, string[]? defaultDirectories = null, bool throwIfFails = false)
        {
            var location = TryGetPrimaryFileLocation();
            var locationId = location.ToShortUUID();
            string? appName = System.IO.Path.GetFileNameWithoutExtension(location);
            if (appName == null || string.IsNullOrEmpty(appName))
            {
                return null;
            }

            if (useStartupAppLocation)
            {
                appName = $"{appName}_{locationId}";
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
                    if (defaultDirectories != null)
                    {
                        foreach (var directory in defaultDirectories)
                        {
                            EnsureWriteableDirectoryExists(result.FullName, directory, throwIfFails);
                        }
                    }
                    return result;
                }
            }

            return null;
        }
    }
}
