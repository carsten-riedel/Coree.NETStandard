using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;
using Coree.NETStandard.Services.HashManagement;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService, HashService>, IFileOperationsService
    {
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
        public async Task<VerifiedCopyStatus> RetryVerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default, int maxRetryCount = 3, int retryDelayMilliseconds = 1000)
        {
            int retryCount = 0;
            while (retryCount < maxRetryCount)
            {
                // Attempt to verify and resume the file copy
                VerifiedCopyStatus result = await VerifyAndResumeFileCopyAsync(source, destination, cancellationToken);

                // Check the result and handle based on the type of error
                switch (result)
                {
                    case VerifiedCopyStatus.Success:
                    case VerifiedCopyStatus.Cancelled:
                    case VerifiedCopyStatus.Error:
                        // If successful, cancelled, or a fundamental error, return immediately
                        return result;

                    case VerifiedCopyStatus.ErrorDuringCopy:
                        // Log the retry attempt if an error occurred during copying
                        _logger?.LogWarning("Attempt {RetryCount} failed with status {Result}. Retrying in {RetryDelayMilliseconds}ms...", retryCount + 1, result, retryDelayMilliseconds);
                        // Wait for a specified delay before retrying
                        await Task.Delay(retryDelayMilliseconds, cancellationToken);
                        break;

                    default:
                        // Log unexpected status
                        _logger?.LogError("Unexpected status {Result} encountered.", result);
                        return result;
                }

                retryCount++; // Increment the retry count
            }

            // If all retries are exhausted, log an error and return the error status
            _logger?.LogError("File copy failed after {MaxRetryCount} attempts.", maxRetryCount);
            return VerifiedCopyStatus.ErrorDuringCopy;
        }

    }
}
