using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {
        /// <summary>
        /// Asynchronously copies a file from a source path to a destination path, overwriting the destination file if it already exists.
        /// </summary>
        /// <param name="source">The file path of the source file to be copied.</param>
        /// <param name="destination">The file path of the destination where the file will be copied.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the file copy operation.</param>
        /// <returns>Returns <c>true</c> if the file is copied successfully. If the file is not copied, the method throws an exception.</returns>
        /// <remarks>
        /// This method wraps the <see cref="RetryVerifyAndResumeFileCopyAsync"/> method to include asynchronous execution with cancellation support and retry logic. It logs the attempt, success, or failure of the file copying process. Use the provided <paramref name="cancellationToken"/> to cancel the operation if needed.
        /// </remarks>
        public async Task<bool> FileCopyAsync(string source, string destination, CancellationToken cancellationToken = default)
        {
            var result = await RetryVerifyAndResumeFileCopyAsync(source, destination, cancellationToken, 3, 3000);

            if (result == VerifiedCopyStatus.Success)
            {
                return true;
            }
            return false;
        }
    }
}