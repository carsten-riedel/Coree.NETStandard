using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

using Microsoft.Extensions.Logging;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {
        /// <summary>
        /// Defines the status outcomes for the VerifyAndResumeFileCopyAsync method.
        /// </summary>
        /// <remarks>
        /// This enumeration provides detailed status outcomes that describe various scenarios encountered during the file verification and copying process.
        /// <code>
        /// <example>
        /// // Example of checking the result of a file copy operation
        /// VerifiedCopyStatus status = await VerifyAndResumeFileCopyAsync(sourcePath, destinationPath);
        /// if (status == VerifiedCopyStatus.Success)
        /// {
        ///     Console.WriteLine("File copy succeeded.");
        /// }
        /// else
        /// {
        ///     Console.WriteLine($"File copy failed with status: {status}");
        /// }
        /// </example>
        /// </code>
        /// </remarks>
        public enum VerifiedCopyStatus
        {
            /// <summary>
            /// Indicates that the file has been successfully verified and copied.
            /// </summary>
            Success,

            /// <summary>
            /// Indicates that the file copy operation was cancelled by the user.
            /// </summary>
            Cancelled,

            /// <summary>
            /// Indicates that an error occurred during the copying process, but not related to starting or completing the copy.
            /// </summary>
            ErrorDuringCopy,

            /// <summary>
            /// Indicates that a general error occurred that prevented the file copy from starting or completing.
            /// </summary>
            Error,

            /// <summary>
            /// Indicates that the file was copied, but metadata associated with the file could not be copied.
            /// </summary>
            NoMetaData,
        }

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
        public async Task<FileOperationsService.VerifiedCopyStatus> VerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default)
        {
            long totalBytesCopied = 0L;

            try
            {
                if (!File.Exists(source))
                {
                    _logger?.LogError("Source file does not exist.");
                    return VerifiedCopyStatus.Error;
                }

                FileInfo fileInfo = new FileInfo(source);
                
                int bufferSize = ChooseFileStreamBufferSize(fileInfo.Length);
                byte[] sourceBuffer = new byte[bufferSize];
                byte[] destBuffer = new byte[bufferSize];

                long position = 0;

                var tempFilenameSuffix = ".temp";
                var tempFilenameSuffixId = _hashService.ComputeCrc32Hash(destination);
                var destinationTempFilenameSuffix = $"{destination}.{tempFilenameSuffixId}.{tempFilenameSuffix}";

                using (FileStream sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize))
                using (FileStream destStream = new FileStream(destinationTempFilenameSuffix, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, bufferSize))
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

                    _logger?.LogInformation("Starting copying source: {source} to destination: {destination}.", source, destination);

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
                    DeleteFile(destination, true);
                    System.IO.File.Move(destinationTempFilenameSuffix, destination);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to move temp file to normal.");
                    return VerifiedCopyStatus.Error;
                }

                try
                {
                    CopyFileAttributes(source, destination, true);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to copy file metadata.");
                    return VerifiedCopyStatus.NoMetaData;
                }

                _logger?.LogInformation("File has been successfully copied to {destination}. Total bytes transferred: {totalBytesCopied}.", destination, totalBytesCopied);
                return VerifiedCopyStatus.Success;
            }
            catch (UnauthorizedAccessException uae)
            {
                _logger?.LogError(uae, "Access to the file was denied.");
                return VerifiedCopyStatus.Error;
            }
            catch (IOException ioe)
            {
                _logger?.LogError(ioe, "A file I/O error occurred during the copy process.");
                return VerifiedCopyStatus.ErrorDuringCopy;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An unexpected error occurred during the verified copy operation.");
                return VerifiedCopyStatus.Error;
            }
        }
    }
}