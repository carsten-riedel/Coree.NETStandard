using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Determines if the current build is a debug build.
        /// </summary>
        /// <returns>A boolean value indicating whether the current build has debugging features enabled, typically signifying a debug build.</returns>
        /// <remarks>
        /// This method checks for the presence of the <see cref="DebuggableAttribute"/> on the entry assembly and examines if JIT tracking is enabled,
        /// which is usually the case for builds compiled in a debug configuration. It's a useful check for altering behavior based on the build configuration
        /// without relying on preprocessor directives.
        /// </remarks>
        public static bool IsDebugBuild()
        {
            var assembly = Assembly.GetEntryAssembly();

            if (assembly.GetCustomAttributes(typeof(DebuggableAttribute), false) is DebuggableAttribute[] attributes && attributes.Length > 0)
            {
                var d = attributes[0];
                if (d.IsJITTrackingEnabled)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
