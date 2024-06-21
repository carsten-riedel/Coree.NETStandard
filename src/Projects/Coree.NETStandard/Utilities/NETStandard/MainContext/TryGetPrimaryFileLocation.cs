using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Coree.NETStandard.Utilities
{
    /// <summary>
    /// Provides utility methods for managing the application's execution context, typically used within the Program.Main entry point.
    /// </summary>
    public static partial class MainContext
    {


        /// <summary>
        /// Attempts to identify and return the primary file location of the currently executing application.
        /// </summary>
        /// <remarks>
        /// This method first tries to match the main executable's name with the current domain name.
        /// If no match is found, it attempts to retrieve the location from the entry assembly or,
        /// if that is not available, from the calling assembly. This method is useful for applications
        /// that need to determine the path of the currently running executable or assembly.
        /// </remarks>
        /// <returns>
        /// The file path of the main module if its name matches the domain, or the location of the
        /// entry or calling assembly. Returns null if none of these locations can be determined.
        /// </returns>
        public static string? TryGetPrimaryFileLocation()
        {
            var mainModuleLocation = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(mainModuleLocation))
            {
                var mainModuleDirectory = System.IO.Path.GetDirectoryName(mainModuleLocation);
                if (mainModuleDirectory.Equals(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                {
                    return mainModuleLocation;
                }
            }

            var primaryOrCallingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var assemblyName = primaryOrCallingAssembly?.Location;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                return assemblyName;
            }

            return null;
        }
    }
}
