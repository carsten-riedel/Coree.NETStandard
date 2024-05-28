using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileManagement
{
    public partial class FileService : ServiceFactoryEx<FileService>, IFileService
    {
        /// <summary>
        /// Checks whether the specified path is a valid file or directory.
        /// </summary>
        /// <param name="path">The path to be checked.</param>
        /// <returns>
        /// <c>true</c> if the path points to an existing file or directory; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method returns <c>false</c> if the provided path is null or empty.
        /// It logs debug messages using the provided logger instance if any errors occur during the validation process.
        /// </remarks>
        public bool IsValidLocation(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                _logger?.LogDebug("Path is null or empty.");
                return false;
            }

            try
            {
                if (File.Exists(path))
                {
                    _logger?.LogTrace("Path '{Path}' points to an existing file.", path);
                    return true;
                }

                if (Directory.Exists(path))
                {
                    _logger?.LogTrace("Path '{Path}' points to an existing directory.", path);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Error while checking path: {Path}", path);
                return false;
            }

            return false;
        }
    }
}