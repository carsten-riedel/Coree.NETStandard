using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;
using Coree.NETStandard.Extensions.Collections.Enumerable;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {

        /// <summary>
        /// Represents an entry within a file system, which can be either a file or a directory.
        /// This class provides detailed information about the file system entry, such as its name,
        /// full path, attributes, and timestamps.
        /// </summary>
        public class FileSystemEntry
        {
            /// <summary>
            /// Gets or sets the full path of the file system entry.
            /// </summary>
            public string? FullName { get; set; }

            /// <summary>
            /// Gets or sets the name of the file system entry.
            /// </summary>
            public string? Name { get; set; }

            /// <summary>
            /// Gets or sets the attributes associated with the file system entry.
            /// </summary>
            public FileAttributes Attributes { get; set; } = FileAttributes.Normal;

            /// <summary>
            /// Gets or sets the CRC32 checksum of the file, if available.
            /// </summary>
            public uint? Crc32 { get; set; }

            /// <summary>
            /// Gets or sets the size of the file system entry in bytes.
            /// </summary>
            public long? Length { get; set; }

            /// <summary>
            /// Gets or sets the last modification time of the entry, expressed in UTC.
            /// </summary>
            public DateTimeOffset LastWriteTimeUtc { get; set; }

            /// <summary>
            /// Gets or sets the time when the entry was indexed or recorded, expressed in UTC.
            /// </summary>
            public DateTimeOffset EntryDateUtc { get; set; }

            /// <summary>
            /// Gets or sets the exception that occurred during the entry processing, if any.
            /// </summary>
            public Exception? Exception { get; set; }

            /// <summary>
            /// Determines whether the file system entry is a file.
            /// This property returns true if the 'Normal' attribute is set.
            /// </summary>
            [JsonIgnore]
            public bool IsFile
            {
                get
                {
                    return Attributes.HasFlag(FileAttributes.Normal);
                }
            }

            /// <summary>
            /// Determines whether the file system entry is a directory.
            /// This property returns true if the 'Directory' attribute is set.
            /// </summary>
            [JsonIgnore]
            public bool IsDirectory
            {
                get
                {
                    return Attributes.HasFlag(FileAttributes.Directory);
                }
            }

            /// <summary>
            /// Gets the file extension of the file system entry, if it is a file.
            /// Returns null if the entry is not a file or if no extension is present.
            /// </summary>
            [JsonIgnore]
            public string? Extension
            {
                get
                {
                    if (IsFile && FullName != null)
                    {
                        return System.IO.Path.GetExtension(FullName);
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Represents detailed information about a specific file system location, including its entries
        /// and statuses related to operations performed on these entries.
        /// </summary>
        public class FileSystemInformation
        {
            /// <summary>
            /// Gets or sets the path of the file system location.
            /// </summary>
            public string? Path { get; set; }

            /// <summary>
            /// Gets or sets the UTC date and time when the file system information was created.
            /// </summary>
            public DateTimeOffset CreatedDateUtc { get; set; } = DateTime.UtcNow;

            /// <summary>
            /// Gets or sets the list of file system entries at the specified path.
            /// </summary>
            public List<FileSystemEntry> FileSystemEntries { get; set; } = new List<FileSystemEntry>();

            /// <summary>
            /// Gets a list of file system entries that did not encounter any exceptions during processing.
            /// </summary>
            [JsonIgnore]
            public List<FileSystemEntry> Ok
            {
                get
                {
                    return FileSystemEntries.Where(e => e.Exception == null).ToList();
                }
            }

            /// <summary>
            /// Gets a list of file system entries that encountered exceptions, excluding those
            /// caused by operation cancellations.
            /// </summary>
            [JsonIgnore]
            public List<FileSystemEntry> Errors
            {
                get
                {
                    return FileSystemEntries.Where(e => e.Exception != null && e.Exception is not OperationCanceledException).ToList();
                }
            }

            /// <summary>
            /// Gets a list of file system entries where the operations were canceled.
            /// This property filters entries with exceptions specifically marked as <see cref="OperationCanceledException"/>.
            /// </summary>
            [JsonIgnore]
            public List<FileSystemEntry> Canceled
            {
                get
                {
                    return FileSystemEntries.Where(e => e.Exception is OperationCanceledException).ToList();
                }
            }

            /// <summary>
            /// Determines whether any file system entries encountered errors during processing,
            /// excluding operation cancellations.
            /// </summary>
            [JsonIgnore]
            public bool HasErrors
            {
                get
                {
                    return Errors.Count > 0;
                }
            }
        }

        /// <summary>
        /// Asynchronously scans the file system entries starting from the specified path, optionally performing CRC32 checks and applying a blacklist filter on file names.
        /// </summary>
        /// <param name="path">The starting path from which to begin scanning for file system entries.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests, which can abort the operation.</param>
        /// <param name="failFast">If set to true, the operation will stop at the first error encountered; otherwise, it will continue despite errors.</param>
        /// <param name="crc32">If set to true, a CRC32 checksum will be calculated for each file.</param>
        /// <param name="fileNameBlacklist">An optional list of file names to exclude from scanning and processing.</param>
        /// <returns>A <see cref="FileSystemInformation"/> object containing all scanned entries, including any errors encountered during the scan.</returns>
        public async Task<FileSystemInformation> ScanFileSystemEntriesAsync(string path, CancellationToken cancellationToken, bool failFast = false, bool crc32 = false, List<string>? fileNameBlacklist = null)
        {
            List<FileSystemEntry> entries = new List<FileSystemEntry>();
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScanFileSystemEntriesAsync(path, cancellationTokenSource, failFast, crc32, path, entries, fileNameBlacklist);
            return new FileSystemInformation() { Path = path, FileSystemEntries = entries };
        }

        /// <summary>
        /// Recursively scans file system entries from a specified base path, handling directory and file entries according to specified conditions and filters.
        /// </summary>
        /// <param name="basePath">The base path from which to recursively scan and process entries.</param>
        /// <param name="cancellationTokenSource">A CancellationTokenSource linked to the original cancellation token, providing a mechanism to cancel the operation.</param>
        /// <param name="failFast">Specifies whether the operation should immediately stop after encountering the first error.</param>
        /// <param name="crc32">Determines whether a CRC32 checksum calculation is required for each file.</param>
        /// <param name="path">The current path being processed. If null, the basePath is used.</param>
        /// <param name="entries">A list to which new file system entries are added as they are scanned.</param>
        /// <param name="fileNameBlacklist">A list of file names to be ignored during the scanning process.</param>
        /// <remarks>
        /// This method is designed to be called recursively, traversing directories and processing files. It checks for cancellation, adheres to a blacklist, and optionally calculates CRC32 checksums, logging errors as it encounters them.
        /// </remarks>
        private async Task ScanFileSystemEntriesAsync(string basePath, CancellationTokenSource cancellationTokenSource, bool failFast = false, bool crc32 = false, string? path = null, List<FileSystemEntry>? entries = null, List<string>? fileNameBlacklist = null)
        {
            entries ??= new List<FileSystemEntry>();
            path ??= basePath;

            List<string> dirs = new List<string>();

            try
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                dirs = System.IO.Directory.GetDirectories(path).ToList();
            }
            catch (OperationCanceledException ex)
            {
                entries.Add(new FileSystemEntry()
                {
                    FullName = path.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                    Exception = ex,
                    Attributes = FileAttributes.Directory,
                });
                return;
            }
            catch (Exception ex)
            {
                entries.Add(new FileSystemEntry()
                {
                    FullName = path.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                    Exception = ex,
                    Attributes = FileAttributes.Directory,
                });
                if (failFast) { cancellationTokenSource.Cancel(); return; }
            }

            foreach (var item in dirs)
            {
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    await ScanFileSystemEntriesAsync(basePath, cancellationTokenSource, failFast, crc32, item, entries,fileNameBlacklist);

                    var directoryInfo = new DirectoryInfo(item);
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = directoryInfo.FullName.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Attributes = FileAttributes.Directory,
                        Name = "",
                        LastWriteTimeUtc = directoryInfo.LastWriteTimeUtc,
                        EntryDateUtc = DateTime.UtcNow,
                        Length = 0,
                        Crc32 = null,
                    }); ;
                }
                catch (OperationCanceledException ex)
                {
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = item.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Exception = ex,
                        Attributes = FileAttributes.Directory,
                    });
                    return;
                }
                catch (Exception ex)
                {
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = item.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Exception = ex,
                        Attributes = FileAttributes.Directory,
                    });
                    if (failFast) { cancellationTokenSource.Cancel(); return; }
                }

                if (entries.Count % 10000 == 0)
                {
                    await Task.Delay(75, cancellationTokenSource.Token);
                }
            }

            List<string> files = new List<string>();
            try
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                //files = await Task.Run(async() => { await Task.Delay(30); List<string> f = new(); try { f.AddRange(Directory.GetFiles(path).ToList()) ; } catch (Exception) { throw; } return f; });
                files = System.IO.Directory.GetFiles(path).ToList();
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (failFast) { cancellationTokenSource.Cancel(); return; }
            }

            foreach (var item in files)
            {
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    var fileInfo = new FileInfo(item);

                    if (fileNameBlacklist != null)
                    {
                        if (fileNameBlacklist.ContainsOrdinalIgnoreCase(fileInfo.Name))
                            continue;
                    }

                    uint? crc32Hash = null;
                    if (crc32)
                    {
                        crc32Hash = await _hashService.ComputeCrc32Async(fileInfo.FullName, cancellationTokenSource.Token);
                    }

                    entries.Add(new FileSystemEntry()
                    {
                        FullName = fileInfo.FullName.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Attributes = FileAttributes.Normal,
                        Name = fileInfo.Name,
                        Length = fileInfo.Length,
                        LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                        EntryDateUtc = DateTime.UtcNow,
                        Crc32 = crc32Hash,
                    });
                }
                catch (OperationCanceledException ex)
                {
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = item.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Exception = ex,
                        Attributes = FileAttributes.Normal,
                    });
                    return;
                }
                catch (Exception ex)
                {
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = item.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Exception = ex,
                        Attributes = FileAttributes.Normal,
                    });
                    if (failFast) { cancellationTokenSource.Cancel(); return; }
                }

                if (entries.Count % 10000 == 0)
                {
                    await Task.Delay(75, cancellationTokenSource.Token);
                }
            }
        }
    }
}