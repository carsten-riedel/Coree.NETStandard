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
        /// Checks if the directory exists or can be created.
        /// </summary>
        /// <param name="baseDirectory">The base directory.</param>
        /// <param name="subDirectory">The assembly name.</param>
        /// <param name="throwIfFails">Indicates whether to throw an exception if the operation fails.</param>
        /// <returns>The full path to the directory, or null if not found or created.</returns>
        private static DirectoryInfo? EnsureWriteableDirectoryExists(string baseDirectory, string subDirectory, bool throwIfFails)
        {
            string path = System.IO.Path.Combine(baseDirectory, subDirectory);
            var dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                if (IsWritableDirectory(dirInfo.FullName))
                {
                    return dirInfo;
                }
                else
                {
                    return null;
                }
            }

            if (IsWritableDirectory(baseDirectory))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(path);
                    return new DirectoryInfo(path);
                }
                catch (Exception ex)
                {
                    if (throwIfFails)
                    {
                        throw new IOException($"Failed to create directory: {path}", ex);
                    }
                }
            }
            return null;
        }
    }
}
