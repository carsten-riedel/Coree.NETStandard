using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.DirectoryOperationsManagement
{
    public partial class DirectoryOperationsService : ServiceFactoryEx<DirectoryOperationsService>, IDirectoryOperationsService
    {
        /// <summary>
        /// Ensures that a specified directory exists. If the directory does not exist, it attempts to create it.
        /// </summary>
        /// <param name="directory">The path of the directory to check and create if necessary.</param>
        /// <returns>true if the directory exists or was successfully created; false if the creation failed.</returns>
        /// <remarks>
        /// This method logs a debug message if the directory creation fails due to an exception.
        /// </remarks>
        /// <example>
        /// <code>
        /// bool isDirectoryEnsured = EnsureDirectory(@"C:\path\to\directory");
        /// Console.WriteLine(isDirectoryEnsured);
        /// </code>
        /// </example>
        public bool EnsureDirectory(string directory)
        {
            // Check if the directory already exists
            if (!System.IO.Directory.Exists(directory))
            {
                try
                {
                    // Attempt to create the directory
                    System.IO.Directory.CreateDirectory(directory);
                }
                catch (Exception ex)
                {
                    // Log debug information if an exception occurs during directory creation
                    _logger?.LogError("EnsureDirectory failed", ex.Message);
                    return false;
                }
            }

            // Return true if the directory exists (either it already existed or was just created)
            return System.IO.Directory.Exists(directory);
        }
    }
}
