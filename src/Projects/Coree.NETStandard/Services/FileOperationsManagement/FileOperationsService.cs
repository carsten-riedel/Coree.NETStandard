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
using System.IO;


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

        /// <summary>
        /// Verifies the integrity of a partially copied file and resumes the copy operation if integrity is confirmed.
        /// </summary>
        /// <param name="source">The source file path.</param>
        /// <param name="destination">The destination file path.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation, with a value indicating the result of the copy operation.</returns>
        /// <remarks>
        /// This method uses a two-phase approach: first, it verifies the existing part of the file in the destination against the source.
        /// If the verification is successful, it resumes copying. If there is a mismatch, it truncates the destination file to the last verified position and resumes the copy.
        /// <example>
        /// <code>
        /// var status = await VerifyAndResumeFileCopyAsync("path/to/source.file", "path/to/destination.file");
        /// if (status == VerifiedCopyStatus.Success)
        /// {
        ///     Console.WriteLine("File copied successfully.");
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        Task<VerifiedCopyStatus> VerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to verify and resume a file copy operation multiple times in case of failures during the copy process.
        /// </summary>
        /// <param name="source">The source file path from which to copy.</param>
        /// <param name="destination">The destination file path to which the file should be copied.</param>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts for the copy operation.</param>
        /// <param name="retryDelayMilliseconds">The delay in milliseconds between retry attempts.</param>
        /// <returns>
        /// A task that represents the asynchronous operation, with a value indicating the result of the copy operation after retries.
        /// </returns>
        /// <remarks>
        /// This method encapsulates a retry logic for the VerifyAndResumeFileCopyAsync method. It retries the file copy operation up to a specified number of times if the error is related to the copy process itself.
        /// Each retry is logged and, if necessary, followed by a specified delay. If the copy operation fails after all retries or is cancelled, the method will return the appropriate status.
        /// <example>
        /// <code>
        /// // Example usage of retrying a file copy operation
        /// var copyStatus = await RetryVerifyAndResumeFileCopyAsync("path/to/source.file", "path/to/destination.file");
        /// if (copyStatus == VerifiedCopyStatus.Success)
        /// {
        ///     Console.WriteLine("File copy succeeded after retries.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"File copy failed with status: {copyStatus}");
        /// }
        /// </code>
        /// </example>
        /// </remarks>
        Task<VerifiedCopyStatus> RetryVerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default, int maxRetryCount = 3, int retryDelayMilliseconds = 1000);



        bool FileCopy(string source,string destination);

        Task<bool> FileCopyAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, CancellationToken cancellationToken = default);

        Task<bool> FileCopierAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, int rewindBlocks = 3, CancellationToken cancellationToken = default);


        

        Task CreateJsonPathInventoryAsync(string? path, string? inventoryFilename = "");

        Task InventoryCopyAsync(string inventoryFilename, string target);

        Task<List<PathInventory>> GetRecursivePathInventoryAsync(DirectoryInfo dirInfo, CancellationToken cancellationToken, List<string>? filenameBlacklist = null, FileVersionHash withFileVersionHash = FileVersionHash.None, bool withFileHash = true, string? partialName = null);

        Task<List<PathInventory2>> StripDownGetRecursivePathInventoryAsync2(DirectoryInfo dirInfo, CancellationToken cancellationToken, List<string>? filenameBlacklist = null, string? partialName = null);

    }
}

