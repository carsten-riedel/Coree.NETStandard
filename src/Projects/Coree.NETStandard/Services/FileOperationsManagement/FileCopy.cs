using System;
using System.Collections.Generic;
using System.IO;
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
        /// Copies a file from a source path to a destination path, overwriting the destination file if it already exists.
        /// </summary>
        /// <param name="source">The file path of the source file to copy.</param>
        /// <param name="destination">The file path of the destination where the file should be copied.</param>
        /// <returns>True if the file was copied successfully; otherwise, an exception is thrown indicating the error.</returns>
        /// <remarks>
        /// This method wraps the <see cref="System.IO.File.Copy"/> method and uses logging to provide detailed information about the operation's progress and outcome.
        /// It logs an attempt and a success message, and errors are logged as exceptions.
        /// </remarks>
        public bool FileCopy(string source, string destination)
        {
            System.IO.FileInfo sourceFileInfo = new System.IO.FileInfo(source);
            if (!sourceFileInfo.Exists)
            {
                _logger?.LogError("Attemting to copy a file that does not exist source dest");
                return false;
            }

            var tempSuffix = ".temporary";
            var tempFileName = $"{destination}.{tempSuffix}";

            try
            {
                DeleteFile(tempFileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError("An unexpected error occurred during the file copy operation.", ex);
            }

            _logger?.LogDebug($"Attempting to copy file from {source} to {destination}.");

            try
            {
               
                System.IO.File.Copy(source, $"{tempFileName}", true);

                DeleteFile(destination);

                System.IO.File.Move(tempFileName, destination);

                _logger?.LogDebug("File copied successfully.");
            }
            catch (Exception ex)
            {
                _logger?.LogError("An unexpected error occurred during the file copy operation.", ex);
            }

            //Cleanup
            try
            {
                DeleteFile(tempFileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError("An unexpected error occurred during the file copy operation.", ex);
            }

            return true;
        }


        public async Task<bool> FileCopyAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, CancellationToken cancellationToken = default)
        {
            FileInfo sourceFileInfo = new FileInfo(source);
            if (!sourceFileInfo.Exists)
            {
                _logger?.LogError("Attempting to copy a file that does not exist: {source}", source);
                return false;
            }

            string tempSuffix = ".temporary";
            string tempFileName = $"{destination}{tempSuffix}";

            for (int attempt = 0; attempt < maxRetryCount; ++attempt)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger?.LogInformation("Operation cancelled.");
                    return false;
                }

                try
                {
                    DeleteFile(tempFileName);

                    await FileCopierAsync(source, tempFileName, 3, 1000, 3, cancellationToken);

                    DeleteFile(destination);

                    // Move from temporary to final destination
                    System.IO.File.Move(tempFileName, destination);

                    _logger?.LogDebug("File copied successfully from {source} to {destination}.", source, destination);
                    return true;
                }
                catch (Exception ex) when (attempt < maxRetryCount - 1)
                {
                    _logger?.LogError(ex, "Attempt {AttemptNumber} failed to copy file from {source} to {destination}. Retrying in {retryDelay} ms...", attempt + 1, source, destination, retryDelay);
                    await Task.Delay(retryDelay, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to copy file from {source} to {destination} after {maxRetryCount} attempts.", source, destination, maxRetryCount);
                    return false;
                }
            }

            // Final attempt to clean up temporary files after retries are exhausted
            try
            {
                DeleteFile(tempFileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to clean up temporary file {tempFileName} after failed copy attempt.", tempFileName);
            }

            return false;
        }


        /// <summary>
        /// Asynchronously copies a file from the source path to the destination path block by block with retry logic.
        /// </summary>
        /// <param name="source">The source file path.</param>
        /// <param name="destination">The destination file path.</param>
        /// <param name="maxRetryCount">Maximum number of retries in case of failure.</param>
        /// <param name="retryDelay">Delay between retries in milliseconds.</param>
        /// <param name="rewindBlocks">Number of blocks to rewind on retry.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <returns>Returns true if the copy was successful; otherwise, false.</returns>
        /// <remarks>
        /// Adjust the buffer size based on memory constraints and expected file size.
        /// </remarks>
        /// <example>
        /// <code>
        /// var success = await new FileCopier(logger).FileCopyAsync("path/to/source.txt", "path/to/destination.txt");
        /// </code>
        /// </example>
        //public async Task<bool> FileCopierAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, int rewindBlocks = 3, CancellationToken cancellationToken = default)
        //{
        //    const int bufferSize = 81920; // 80 KB buffer size, adjust as needed.
        //    byte[] buffer = new byte[bufferSize];
        //    long totalBytesCopied = 0;
        //    int currentRetry = 0;
        //    bool isCopySuccessful = false;

        //    _logger?.LogDebug("Starting file copy from {source} to {destination}.", source, destination);

        //    try
        //    {
        //        using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
        //        using (var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
        //        {
        //            _logger?.LogTrace("Opened file streams.");
        //            while (true)
        //            {
        //                int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        //                if (bytesRead == 0)
        //                {
        //                    isCopySuccessful = true;
        //                    _logger?.LogDebug("File copy completed successfully.");
        //                    break; // End of file reached
        //                }
        //                await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        //                totalBytesCopied += bytesRead;
        //                _logger?.LogTrace("Copied {bytesRead} bytes, total {totalBytesCopied} bytes copied.", bytesRead, totalBytesCopied);
        //            }
        //        }
        //    }
        //    catch (Exception ex) when (ex is IOException || ex is OperationCanceledException)
        //    {
        //        _logger?.LogWarning("Copy operation failed with exception: {message}. Retrying...", ex.Message);
        //        while (currentRetry < maxRetryCount && !cancellationToken.IsCancellationRequested)
        //        {
        //            try
        //            {
        //                currentRetry++;
        //                _logger?.LogDebug("Retry {currentRetry} of {maxRetryCount}.", currentRetry, maxRetryCount);
        //                using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
        //                using (var destinationStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, true))
        //                {
        //                    long rewindPosition = totalBytesCopied - rewindBlocks * bufferSize;
        //                    if (rewindPosition < 0) rewindPosition = 0;

        //                    sourceStream.Seek(rewindPosition, SeekOrigin.Begin);
        //                    destinationStream.Seek(rewindPosition, SeekOrigin.Begin);
        //                    destinationStream.SetLength(destinationStream.Position); // Truncate to remove faulty blocks

        //                    _logger?.LogDebug("Rewound to {rewindPosition} bytes.", rewindPosition);

        //                    while (true)
        //                    {
        //                        int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        //                        if (bytesRead == 0)
        //                        {
        //                            isCopySuccessful = true;
        //                            _logger?.LogDebug("File copy resumed and completed successfully after retries.");
        //                            break; // End of file reached
        //                        }
        //                        await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
        //                        totalBytesCopied += bytesRead;
        //                        _logger?.LogTrace("Resumed copy: Copied {bytesRead} bytes, total {totalBytesCopied} bytes copied.", bytesRead, totalBytesCopied);
        //                    }
        //                }
        //            }
        //            catch (Exception retryEx) when (retryEx is IOException || retryEx is OperationCanceledException)
        //            {
        //                _logger?.LogError("Retry failed with exception: {message}.", retryEx.Message);
        //                if (currentRetry >= maxRetryCount)
        //                {
        //                    _logger?.LogCritical("Maximum retry limit reached, copying failed.");
        //                    break;
        //                }
        //                await Task.Delay(retryDelay, cancellationToken);
        //            }
        //        }
        //    }

        //    return isCopySuccessful;
        //}

        /// <summary>
        /// Asynchronously copies a file from the source path to the destination path block by block with retry and rewind logic,
        /// and clean up for incomplete files in case of failure, distinct handling of cancellation.
        /// </summary>
        /// <param name="source">The source file path.</param>
        /// <param name="destination">The destination file path.</param>
        /// <param name="maxRetryCount">Maximum number of retries in case of failure.</param>
        /// <param name="retryDelay">Delay between retries in milliseconds.</param>
        /// <param name="rewindBlocks">Number of blocks to rewind on retry.</param>
        /// <param name="cancellationToken">Token for cancellation.</param>
        /// <returns>Returns true if the copy was successful; otherwise, false.</returns>
        /// <remarks>
        /// Adjust the buffer size based on memory constraints and expected file size.
        /// </remarks>
        /// <example>
        /// <code>
        /// var success = await new FileCopier(logger).FileCopyAsync("path/to/source.txt", "path/to/destination.txt");
        /// </code>
        /// </example>
        public async Task<bool> FileCopierAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, int rewindBlocks = 3, CancellationToken cancellationToken = default)
        {
            const int bufferSize = 81920; // 80 KB buffer size, adjust as needed.
            byte[] buffer = new byte[bufferSize];
            long totalBytesCopied = 0;
            int currentRetry = 0;
            bool isCopySuccessful = false;

            _logger?.LogDebug("Starting file copy from {source} to {destination}.", source, destination);

            try
            {
                using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
                using (var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true))
                {
                    _logger?.LogTrace("Opened file streams.");
                    while (true)
                    {
                        int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                        if (bytesRead == 0)
                        {
                            isCopySuccessful = true;
                            _logger?.LogDebug("File copy completed successfully.");
                            break; // End of file reached
                        }
                        await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                        totalBytesCopied += bytesRead;
                        _logger?.LogTrace("Copied {bytesRead} bytes, total {totalBytesCopied} bytes copied.", bytesRead, totalBytesCopied);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger?.LogInformation("Operation canceled by user.");
                // No need to retry or delete file, just exit the method.
                return false;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning("Copy operation failed with exception: {message}. Attempting retries...", ex.Message);
                while (currentRetry < maxRetryCount && !cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        currentRetry++;
                        _logger?.LogDebug("Retry {currentRetry} of {maxRetryCount}.", currentRetry, maxRetryCount);
                        using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
                        using (var destinationStream = new FileStream(destination, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, bufferSize, true))
                        {
                            long rewindPosition = totalBytesCopied - rewindBlocks * bufferSize;
                            if (rewindPosition < 0) rewindPosition = 0;

                            sourceStream.Seek(rewindPosition, SeekOrigin.Begin);
                            destinationStream.Seek(rewindPosition, SeekOrigin.Begin);
                            destinationStream.SetLength(destinationStream.Position); // Truncate to remove faulty blocks

                            _logger?.LogDebug("Rewound to {rewindPosition} bytes.", rewindPosition);

                            while (true)
                            {
                                int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                                if (bytesRead == 0)
                                {
                                    isCopySuccessful = true;
                                    _logger?.LogDebug("File copy resumed and completed successfully after retries.");
                                    break; // End of file reached
                                }
                                await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                                totalBytesCopied += bytesRead;
                                _logger?.LogTrace("Resumed copy: Copied {bytesRead} bytes, total {totalBytesCopied} bytes copied.", bytesRead, totalBytesCopied);
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        _logger?.LogInformation("Operation canceled during retry.");
                        break; // Exit retry loop on cancellation.
                    }
                    catch (Exception retryEx)
                    {
                        _logger?.LogError("Retry failed with exception: {message}.", retryEx.Message);
                        if (currentRetry >= maxRetryCount)
                        {
                            _logger?.LogCritical("Maximum retry limit reached, copying failed.");
                            break;
                        }
                        await Task.Delay(retryDelay, cancellationToken);
                    }
                }
            }
            finally
            {
                if (!isCopySuccessful && File.Exists(destination))
                {
                    try
                    {
                        File.Delete(destination);
                        _logger?.LogDebug("Partial file deleted at {destination}.", destination);
                    }
                    catch (Exception delEx)
                    {
                        _logger?.LogError("Failed to delete partial file at {destination}: {message}", destination, delEx.Message);
                    }
                }
            }

            return isCopySuccessful;
        }

        /// Enum to describe the result of the copy operation.

    }
}

