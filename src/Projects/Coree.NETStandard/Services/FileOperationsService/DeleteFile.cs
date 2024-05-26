using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileOperationsService
{
    public partial class FileOperationsService : ServiceFactory<FileOperationsService>, IFileOperationsService
    {

        /// <summary>
        /// Attempts to delete a file at a specified location.
        /// </summary>
        /// <param name="filePath">The path of the file to be deleted.</param>
        /// <returns>True if the file was deleted successfully or did not exist; false if an error occurred during deletion.</returns>
        public bool DeleteFile(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                try
                {
                    System.IO.File.Delete(filePath);
                    _logger?.LogDebug($"File deleted successfully at {filePath}.");
                    return true;
                }
                catch
                {
                    throw;
                }
            }
            else
            {
                _logger?.LogDebug($"Attempt to delete a non-existent file at {filePath}. No action needed.");
                return true;
            }
        }

    }
}
