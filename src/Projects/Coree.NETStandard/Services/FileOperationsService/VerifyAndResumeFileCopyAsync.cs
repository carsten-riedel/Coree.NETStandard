using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileOperationsService
{
    public partial class FileOperationsService : ServiceFactory<FileOperationsService>, IFileOperationsService
    {

        public enum VerifiedCopyStatus
        {
            Success,
            Cancelled,
            ErrorDuringCopy,
            FundamentalError
        }

        /// <summary>
        /// Performs a verified copy of a file from source to destination, ensuring data integrity by comparing existing contents and resuming from the first mismatch.
        /// </summary>
        /// <param name="source">The source file path.</param>
        /// <param name="destination">The destination file path.</param>
        /// <param name="cancellationToken">Optional cancellation token to cancel the operation.</param>
        /// <returns>A FileCopyStatus enum indicating the result of the copy operation.</returns>
        /// <remarks>
        /// This method checks for data integrity up to the last byte of the existing destination file and resumes copying only from the first discrepancy.
        /// </remarks>
        public async Task<VerifiedCopyStatus> VerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default)
        {
            const int bufferFactor = 4;
            const int bufferSize = (16 * 2 * 4 * 1024) * bufferFactor;
            const int streamBufferSize = (2 * 4 * 1024) * bufferFactor;

            byte[] sourceBuffer = new byte[bufferSize];
            byte[] destBuffer = new byte[bufferSize];
            long totalBytesCopied = 0L;

            try
            {
                if (!File.Exists(source))
                {
                    _logger?.LogError("Source file does not exist.");
                    return VerifiedCopyStatus.FundamentalError;
                }

                

                long position = 0;

                using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, streamBufferSize))
                using (FileStream destStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, streamBufferSize))
                {
                    long sourceLength = sourceStream.Length;
                    long destLength = destStream.Length;

                    if (position < destLength)
                    {
                        _logger?.LogDebug("Starting to compare existing contents up to {length} bytes.", destLength);
                        _logger?.LogInformation("Initiating verification and copy from source: {source} to destination: {destination}.", source, destination);
                    }

                    // Read and compare existing contents
                    while (position < destLength)
                    {
                        int sourceBytesRead = await sourceStream.ReadAsync(sourceBuffer, 0, sourceBuffer.Length, cancellationToken);
                        int destBytesRead = await destStream.ReadAsync(destBuffer, 0, destBuffer.Length, cancellationToken);

                        if (!sourceBuffer.AsSpan(0, sourceBytesRead).SequenceEqual(destBuffer.AsSpan(0, destBytesRead)))
                        {
                            _logger?.LogWarning("Mismatch detected at position {position}. Preparing to truncate and resume copy.", position);
                            // Mismatch handling logic here
                            break;
                        }

                        position += sourceBytesRead;
                        _logger?.LogDebug("Verified {position} bytes so far, no discrepancies found.", position);

                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger?.LogWarning("Verification was cancelled by the user.");
                            return VerifiedCopyStatus.Cancelled;
                        }
                    }

                    // Adjust destination stream length to position
                    destStream.SetLength(position);
                    sourceStream.Seek(position, SeekOrigin.Begin);
                    destStream.Seek(position, SeekOrigin.Begin);

                    _logger?.LogInformation("Starting copying source: {source} to destination: {destination}.",source,destination);

                    int bytesRead;
                    while ((bytesRead = await sourceStream.ReadAsync(sourceBuffer, 0, sourceBuffer.Length, cancellationToken)) > 0)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            _logger?.LogWarning("Copy operation was cancelled by the user.");
                            return VerifiedCopyStatus.Cancelled;
                        }

                        await destStream.WriteAsync(sourceBuffer, 0, bytesRead, cancellationToken);
                        destStream.Flush();
                        totalBytesCopied += bytesRead;
                        _logger?.LogDebug("Current position: {position} bytes. Total bytes copied in this session: {totalBytesCopied}.", destStream.Position, totalBytesCopied);
                    }
                }

                try
                {
                    var sourceFileInfo = new FileInfo(source);
                    var destFileInfo = new FileInfo(destination);
                    File.SetAttributes(destination, sourceFileInfo.Attributes);
                    File.SetCreationTime(destination, File.GetCreationTime(source));
                    File.SetLastAccessTime(destination, File.GetLastAccessTime(source));
                    File.SetLastWriteTime(destination, File.GetLastWriteTime(source));
                    _logger?.LogDebug("Successfully copied metadata from source to destination.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to copy file metadata.");
                    return VerifiedCopyStatus.FundamentalError;
                }

                _logger?.LogInformation("File has been successfully copied to {destination}. Total bytes transferred: {totalBytesCopied}.", destination, totalBytesCopied);
                return VerifiedCopyStatus.Success;
            }
            catch (UnauthorizedAccessException uae)
            {
                _logger?.LogError(uae, "Access to the file was denied.");
                return VerifiedCopyStatus.FundamentalError;
            }
            catch (IOException ioe)
            {
                _logger?.LogError(ioe, "A file I/O error occurred during the copy process.");
                return VerifiedCopyStatus.ErrorDuringCopy;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An unexpected error occurred during the verified copy operation.");
                return VerifiedCopyStatus.FundamentalError;
            }
        }


    }
}
