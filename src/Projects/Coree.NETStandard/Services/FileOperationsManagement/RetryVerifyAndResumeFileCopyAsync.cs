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
