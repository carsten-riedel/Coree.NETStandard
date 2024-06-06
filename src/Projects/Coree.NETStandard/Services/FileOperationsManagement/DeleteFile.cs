using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;
using Coree.NETStandard.Services.HashManagement;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {

        /// <summary>
        /// Attempts to delete a file at a specified location. It logs the operation and can optionally throw an exception on error.
        /// </summary>
        /// <param name="filePath">The path of the file to be deleted.</param>
        /// <param name="throwOnError">Specifies whether to throw an exception if the deletion fails due to an error.</param>
        /// <returns>True if the file was deleted successfully or did not exist; false if an error occurred during deletion.</returns>
        public bool DeleteFile(string filePath, bool throwOnError = false)
        {
            if (!System.IO.File.Exists(filePath))
            {
                _logger?.LogTrace($"File already deleted or does not exist at {filePath}. No action needed.");
                return true;
            }

            try
            {
                System.IO.File.Delete(filePath);
                _logger?.LogTrace($"File deleted successfully at {filePath}.");
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error deleting file at '{filePath}': {ex.Message}";
                _logger?.LogError(ex, errorMessage);
                if (throwOnError)
                {
                    throw;
                }
                else
                {
                    return false;
                }
            }
        }

    }
}
