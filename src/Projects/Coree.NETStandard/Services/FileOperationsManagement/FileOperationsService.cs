using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Microsoft.Extensions.Logging;
using Coree.NETStandard.Services.HashManagement;
using static Coree.NETStandard.Services.FileOperationsManagement.FileOperationsService;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;


namespace Coree.NETStandard.Services.FileOperationsManagement
{
    /// <summary>
    /// Provides comprehensive file management services.
    /// </summary>
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService, HashService>, IFileOperationsService
    {
        private readonly ILogger<FileOperationsService>? _logger;
        private readonly HashService _hashService;

        /// <summary>
        /// Initializes a new instance of the FileOperationsService with necessary dependencies.
        /// </summary>
        /// <param name="hashService">Provides the hashing services required for file operations.</param>
        /// <param name="logger">The logger used to log service activity and errors, if any.</param>
        public FileOperationsService(HashService hashService, ILogger<FileOperationsService>? logger = null)
        {
            this._hashService = hashService;
            this._logger = logger;
        }
    }

    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    public interface IFileOperationsService
    {
        /// <summary>
        /// Copies the file attributes and UTC timestamps from a source file to a destination file. 
        /// This method provides an option to control exception handling based on the failure conditions encountered during the operation.
        /// </summary>
        /// <param name="source">The source file path. This must point to an existing file, or the method will handle the error based on the throwOnError parameter.</param>
        /// <param name="destination">The destination file path. This must point to an existing file, or the method will handle the error based on the throwOnError parameter.</param>
        /// <param name="throwOnError">Indicates whether to throw an exception when the operation fails due to the non-existence of the source or destination, or due to other errors during the copying process. If set to false, the method logs a warning and returns false instead of throwing.</param>
        /// <returns>True if the file metadata was successfully copied; otherwise, false. If false is returned, check logs for the specific failure reason.</returns>
        bool CopyFileAttributes(string source, string destination, bool throwOnError = false);

        /// <summary>
        /// Attempts to delete a file at a specified location. It logs the operation and can optionally throw an exception on error.
        /// </summary>
        /// <param name="filePath">The path of the file to be deleted.</param>
        /// <param name="throwOnError">Specifies whether to throw an exception if the deletion fails due to an error.</param>
        /// <returns>True if the file was deleted successfully or did not exist; false if an error occurred during deletion.</returns>
        bool DeleteFile(string filePath, bool throwOnError = false);

        bool FileCopy(string source,string destination);

        Task<bool> FileCopyAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, CancellationToken cancellationToken = default);

        Task<bool> FileCopierAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, int rewindBlocks = 3, CancellationToken cancellationToken = default);

        

        Task<VerifiedCopyStatus> RetryVerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default, int maxRetryCount = 3, int retryDelayMilliseconds = 1000);

        Task<VerifiedCopyStatus> VerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default);

        Task CreateJsonPathInventoryAsync(string? path, string? inventoryFilename = "");

        Task InventoryCopyAsync(string inventoryFilename, string target);

    }
}

