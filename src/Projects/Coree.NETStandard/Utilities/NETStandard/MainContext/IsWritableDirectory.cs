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
        /// Checks if the directory is writable.
        /// </summary>
        /// <param name="directory">The directory path.</param>
        /// <returns>True if the directory is writable, otherwise false.</returns>
        private static bool IsWritableDirectory(string directory)
        {
            try
            {
                using (FileStream fs = File.Create(System.IO.Path.Combine(directory, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
