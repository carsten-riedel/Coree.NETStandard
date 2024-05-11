using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileService
{
    public partial class FileService : ServiceFactory<FileService>, IFileService
    {

        /// <summary>
        /// Retrieves a list of valid and unique directory paths from the system's PATH environment variable.
        /// </summary>
        /// <returns>A list of strings representing valid, unique, and normalized directory paths from the PATH environment. Returns an empty list if the PATH environment variable is not accessible or if no valid paths are found.</returns>
        /// <remarks>
        /// This method processes each entry in the PATH environment variable by:
        /// - Normalizing the casing of each path using the TryFixPathCaseing method.
        /// - Checking each path for existence to ensure validity.
        /// - Ensuring that each path is unique within the context of the PATH variable to avoid duplicates.
        /// - Logging various statuses such as inaccessible PATH, skipped entries due to invalidity or duplication, and errors during processing.
        /// The method is robust against non-existent directories, permission issues, and other filesystem anomalies by logging and skipping over problematic entries.
        /// </remarks>
        /// <example>
        /// <code>
        /// var validPaths = GetValidUniquePaths();
        /// foreach (var path in validPaths)
        /// {
        ///     Console.WriteLine(path); // Prints each valid and unique path
        /// }
        /// </code>
        /// </example>
        public List<string> GetValidUniquePaths()
        {
            var pathVariable = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (pathVariable == null)
            {
                _logger?.LogDebug("The PATH environment variable is not set or cannot be accessed.");
                return new List<string>();
            }

            // Split the PATH environment variable and process each path
            var pathDirectoryList = pathVariable.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
            var uniqueNormalizedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var validPaths = new List<string>();

            foreach (var path in pathDirectoryList)
            {
                string? normalizedPath;
                try
                {
                    // Normalize the path and correct its casing using the TryFixPathCaseing method
                    normalizedPath = TryFixPathCaseing(path);
                    if (normalizedPath == null || string.IsNullOrEmpty(normalizedPath) || !Directory.Exists(normalizedPath))
                    {
                        _logger?.LogDebug($"Invalid or non-existent PATH entry, skipping: {path}");
                        continue;
                    }

                    // Add the path to the list if it's not a duplicate
                    if (uniqueNormalizedPaths.Add(normalizedPath))
                    {
                        validPaths.Add(normalizedPath);
                    }
                    else
                    {
                        _logger?.LogDebug($"Duplicate PATH entry detected, skipping: {normalizedPath}");
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogDebug($"Error processing PATH entry '{path}'. Error: {ex.Message}");
                }
            }

            _logger?.LogDebug("Valid and unique PATH entries retrieved.");
            return validPaths;
        }
    }
}
