using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Microsoft.Extensions.Logging;
using System.IO;
using Coree.NETStandard.Services.HashManagement;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService, HashService>, IFileOperationsService
    {
        /// <summary>
        /// Copies the file attributes and UTC timestamps from a source file to a destination file. 
        /// This method provides an option to control exception handling based on the failure conditions encountered during the operation.
        /// </summary>
        /// <param name="source">The source file path. This must point to an existing file, or the method will handle the error based on the throwOnError parameter.</param>
        /// <param name="destination">The destination file path. This must point to an existing file, or the method will handle the error based on the throwOnError parameter.</param>
        /// <param name="throwOnError">Indicates whether to throw an exception when the operation fails due to the non-existence of the source or destination, or due to other errors during the copying process. If set to false, the method logs a warning and returns false instead of throwing.</param>
        /// <returns>True if the file metadata was successfully copied; otherwise, false. If false is returned, check logs for the specific failure reason.</returns>
        public bool CopyFileAttributes(string source, string destination, bool throwOnError = false)
        {
            if (!File.Exists(source))
            {
                string errorMessage = $"Source file does not exist: {source}";
                if (throwOnError)
                {
                    _logger?.LogError(errorMessage);
                    throw new FileNotFoundException(errorMessage);
                }
                else
                {
                    _logger?.LogWarning(errorMessage);
                }
                return false;
            }

            if (!File.Exists(destination))
            {
                string errorMessage = $"Destination file does not exist: {destination}";
                if (throwOnError)
                {
                    _logger?.LogError(errorMessage);
                    throw new FileNotFoundException(errorMessage);
                }
                else
                {
                    _logger?.LogWarning(errorMessage);
                }
                return false;
            }

            try
            {
                var sourceFileInfo = new FileInfo(source);
                var destFileInfo = new FileInfo(destination);
                File.SetAttributes(destination, sourceFileInfo.Attributes);
                File.SetCreationTimeUtc(destination, sourceFileInfo.CreationTimeUtc);
                File.SetLastAccessTimeUtc(destination, sourceFileInfo.LastAccessTimeUtc);
                File.SetLastWriteTimeUtc(destination, sourceFileInfo.LastWriteTimeUtc);

                _logger?.LogTrace("Metadata successfully copied from '{Source}' to '{Destination}'.", source, destination);
                return true;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error copying attributes from '{source}' to '{destination}': {ex.Message}";
                if (throwOnError)
                {
                    _logger?.LogError(ex, errorMessage, source, destination);
                    throw;
                }
                else
                {
                    _logger?.LogWarning(errorMessage, source, destination);
                }
                return false;
            }
        }


    }
}
