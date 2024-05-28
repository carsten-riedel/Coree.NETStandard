using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileManagement
{
    public partial class FileService : ServiceFactoryEx<FileService>, IFileService
    {
        /// <summary>
        /// Checks if a specified command is available in the current directory or any of the directories listed in the system's PATH environment variable.
        /// </summary>
        /// <param name="command">The name of the executable file to search for.</param>
        /// <returns>The full path of the executable file with corrected casing if found; otherwise, returns null. The method returns null if the command parameter is null, or if the executable cannot be found in the current or PATH directories.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the command input.
        /// - Retrieves the current directory and the directories from the system's PATH environment using the GetValidUniquePaths method, which excludes invalid and duplicate paths.
        /// - Checks each directory for the existence of the specified command.
        /// - Utilizes the TryFixPathCaseing method to return the path in the correct casing.
        /// - Logs various informational messages during the process, including errors encountered, invalid path entries, and successful detection of the executable.
        /// This method ensures that the search includes the current directory, aligning with common command-line behaviors where the current directory is typically searched before the PATH directories.
        /// </remarks>
        /// <example>
        /// <code>
        /// string commandName = "example";
        /// string executablePath = IsCommandAvailable(commandName);
        /// if (executablePath != null)
        /// {
        ///     Console.WriteLine($"Executable found: {executablePath}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Executable not found.");
        /// }
        /// </code>
        /// </example>
        public string? IsCommandAvailable(string? command)
        {
            if (command == null)
            {
                _logger?.LogDebug("Command is null.");
                return null;
            }

            // Include the current directory in the list of paths to check
            var currentDirectory = Directory.GetCurrentDirectory();
            var pathDirectoryList = GetValidUniquePaths();
            pathDirectoryList.Insert(0, currentDirectory);  // Prepend the current directory to ensure it's checked first

            foreach (var path in pathDirectoryList)
            {
                try
                {
                    if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                    {
                        _logger?.LogInformation($"The PATH contains an invalid or non-existent entry, skipping: {path}");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogInformation($"Error correcting path case or checking directory existence for '{path}'. Error: {ex.Message}");
                    continue;
                }

                // Check if the executable file is present in the corrected path
                var executablePath = IsExecutableFilePresent(command, path);
                if (executablePath != null)
                {
                    _logger?.LogDebug($"Executable found: {executablePath}");
                    return TryFixPathCaseing(executablePath);  // Return the path to the executable if found, with correct casing
                }
            }

            _logger?.LogDebug("No executable found for the provided command in any PATH entry.");
            return null;
        }


    }
}