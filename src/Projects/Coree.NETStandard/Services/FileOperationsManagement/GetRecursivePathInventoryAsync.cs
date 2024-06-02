using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;
using Coree.NETStandard.Services.HashManagement;
using System.Text.Json.Serialization;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService, HashService>, IFileOperationsService
    {
        public class PathInventory
        {
            public string? FileVersionHash { get; set; }

            public string? FileHash { get; set; }

            public string? FullName { get; set; }

            public string? PartialName { get; set; }

            public string? Name { get; set; }

            public string? RootDir { get; set; }

            public string? Extension { get; set; }

            public bool IsDirectory { get; set; }

            public bool IsFile { get; set; }

            public long Length { get; set; } = 0;

            public DateTime LastWriteTimeUtc { get; set; }

            public DateTime EntryDate { get; set; }

            public bool isOk { get; set; } = false;

            public string? ExceptionMessage { get; set; }
        }

        public class PathInventory2
        {
            public uint? FileCrc32 { get; set; }

            public string? FullName { get; set; }

            public string? PartialName { get; set; }

            public string? Name { get; set; }

            public string? RootDir { get; set; }

            public string? Extension { get; set; }

            public bool IsDirectory { get; set; }

            public bool IsFile { get; set; }

            public long Length { get; set; } = 0;

            public DateTime LastWriteTimeUtc { get; set; }

            public DateTime EntryDate { get; set; }

            public bool isOk { get; set; } = false;

            public string? ExceptionMessage { get; set; }
        }

        public class Entry
        {
            public string? RootItem { get; set; }
            public string? FullName { get; set; }
            public string? Name { get; set; }
            public int? Attributes { get; set; }
            public uint? Crc32 { get; set; }
            public long? Length { get; set; }
            public DateTimeOffset LastWriteTimeUtc { get; set; }
            public DateTimeOffset EntryDateUtc { get; set; }
            
            [JsonIgnore]
            public bool? IsFile
            {
                get { 
                    if (Length == null)
                    {
                        return null;
                    }
                    else if (Length != 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            [JsonIgnore]
            public bool? IsDirectoy
            {
                get
                {
                    if (Length == null)
                    {
                        return null;
                    }
                    else if (Length == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            [JsonIgnore]
            public string? Extension
            {
                get
                {
                    if (FullName == null)
                    {
                        return null;
                    }
                    if (IsFile == null)
                    {
                        return null;
                    }
                    else if (IsFile == false)
                    {
                        return null;
                    }
                    else 
                    {
                        return System.IO.Path.GetExtension(FullName);
                    }
                }
            }

        }

        private static async Task<string?> ComputeMD5StringAsync(string input, CancellationToken cancellationToken)
        {
            // Convert the string to a byte array using Unicode encoding
            byte[] inputBytes = Encoding.Unicode.GetBytes(input);

            // Create a MemoryStream around the byte array
            using (MemoryStream memoryStream = new MemoryStream(inputBytes))
            {
                // Compute the MD5 hash of the memory stream
                return await ComputeMD5Async(memoryStream, cancellationToken);
            }
        }

        // Overload for file path
        private static async Task<string?> ComputeMD5FileAsync(string filePath, CancellationToken cancellationToken)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            {
                return await ComputeMD5StreamAsync(stream, cancellationToken);
            }
        }

        // Overload for MemoryStream
        private static async Task<string?> ComputeMD5Async(MemoryStream memoryStream, CancellationToken cancellationToken)
        {
            // Reset memory stream position to ensure correct data hashing
            memoryStream.Position = 0;
            return await ComputeMD5StreamAsync(memoryStream, cancellationToken);
        }

        private static async Task<string?> ComputeMD5StreamAsync(Stream inputStream, CancellationToken cancellationToken)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    md5.TransformFinalBlock(buffer, 0, 0);

                    return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("MD5 computation was canceled.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during MD5 computation: {ex.Message}");
                return null;
            }
        }


        public enum FileVersionHash
        {
            None,
            All,
            Exedll,
        }

        // Define a HashSet for allowed extensions with a case-insensitive comparer
        private static readonly HashSet<string> allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".exe", ".dll", ""
        };

        public async Task<List<PathInventory>> GetRecursivePathInventoryAsync(DirectoryInfo dirInfo, CancellationToken cancellationToken, List<string>? filenameBlacklist = null, FileVersionHash withFileVersionHash = FileVersionHash.None, bool withFileHash = true, string? partialName = null)
        {
            List<PathInventory> list = new List<PathInventory>();
            partialName ??= dirInfo.FullName;
            filenameBlacklist ??= new List<string>();
            HashSet<string>? filenameBlacklistHashSet = null;
            if (filenameBlacklist != null)
            {
                filenameBlacklistHashSet = new HashSet<string>(filenameBlacklist, StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                // Asynchronously add all files in the current directory to the list.
                var files = dirInfo.EnumerateFiles();
                foreach (var fileInfo in files)
                {
                    if (filenameBlacklistHashSet != null)
                    {
                        if (filenameBlacklistHashSet.Contains(fileInfo.Name))
                            continue; // Skip files that are in the blacklist
                    }

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
                        string? versionHash = null;
                        string? fileHash = null;

                        switch (withFileVersionHash)
                        {
                            case FileVersionHash.All:
                                FileVersionInfo fileVersionInfoall = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
                                versionHash = await ComputeMD5StringAsync(fileVersionInfoall.ToString(), cancellationToken);
                                break;

                            case FileVersionHash.Exedll:
                                if (allowedExtensions.Contains(fileInfo.Extension))
                                {
                                    FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
                                    versionHash = await ComputeMD5StringAsync(fileVersionInfo.ToString(), cancellationToken);
                                }
                                break;

                            default:
                                break;
                        }

                        if (withFileHash)
                        {
                            fileHash = await ComputeMD5FileAsync(fileInfo.FullName, cancellationToken);
                        }

                        list.Add(new PathInventory
                        {
                            FullName = fileInfo.FullName,
                            PartialName = fileInfo.FullName.Substring(partialName.Length),
                            Name = fileInfo.Name,
                            RootDir = partialName,
                            Extension = fileInfo.Extension,
                            IsDirectory = false,
                            IsFile = true,
                            Length = fileInfo.Length,
                            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                            EntryDate = DateTime.UtcNow,
                            FileVersionHash = versionHash,
                            FileHash = fileHash,
                            isOk = true
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation was canceled. Partial results will be returned.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {fileInfo.FullName}: {ex.Message}");
                        // Continue processing other files despite the error
                        list.Add(new PathInventory
                        {
                            FullName = fileInfo.FullName,
                            Name = fileInfo.Name,
                            PartialName = fileInfo.FullName.Substring(partialName.Length),
                            RootDir = partialName,
                            Extension = fileInfo.Extension,
                            IsDirectory = false,
                            IsFile = true,
                            Length = fileInfo.Length,
                            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                            EntryDate = DateTime.UtcNow,
                            FileVersionHash = null,
                            FileHash = null,
                            isOk = false,
                            ExceptionMessage = ex.Message
                        });
                    }

                    // Introduce a delay every 5000 entries to avoid overwhelming resources
                    if (list.Count % 5000 == 0)
                    {
                        await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
                    }
                }

                // Asynchronously add directories and recurse.
                var directories = dirInfo.EnumerateDirectories();
                foreach (var subDirInfo in directories)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
                        list.Add(new PathInventory
                        {
                            FullName = subDirInfo.FullName,
                            PartialName = subDirInfo.FullName.Substring(partialName.Length),
                            Name = subDirInfo.Name,
                            RootDir = partialName,
                            Extension = subDirInfo.Extension,
                            IsDirectory = true,
                            IsFile = false,
                            LastWriteTimeUtc = subDirInfo.LastWriteTimeUtc,
                            EntryDate = DateTime.UtcNow,
                            Length = 0,
                            isOk = true
                        });

                        var recursiveList = await GetRecursivePathInventoryAsync(subDirInfo, cancellationToken, filenameBlacklist, withFileVersionHash, withFileHash, partialName); // Recursive async call
                        list.AddRange(recursiveList); // Combine results from the recursion
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation was canceled. Partial results will be returned.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing directory {subDirInfo.FullName}: {ex.Message}");
                        // Continue processing other directories despite the error
                    }

                    // Delay again every 5000 entries
                    if (list.Count % 5000 == 0)
                    {
                        await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled. Partial results will be returned.");
            }
            catch (Exception ex)
            {
                // Log other errors as needed
                Console.WriteLine($"Error processing: {ex.Message}");
                list.Add(new PathInventory
                {
                    FullName = dirInfo.FullName,
                    PartialName = dirInfo.FullName.Substring(partialName.Length),
                    Name = dirInfo.Name,
                    RootDir = partialName,
                    Extension = dirInfo.Extension,
                    IsDirectory = true,
                    IsFile = false,
                    LastWriteTimeUtc = dirInfo.LastWriteTimeUtc,
                    EntryDate = DateTime.UtcNow,
                    Length = 0,
                    isOk = false,
                    ExceptionMessage = ex.Message
                });
            }
            return list; // Return the partial list gathered until the point of cancellation or complete list if not canceled
        }

        public async Task<List<PathInventory2>> StripDownGetRecursivePathInventoryAsync2(DirectoryInfo dirInfo, CancellationToken cancellationToken, List<string>? filenameBlacklist = null, string? partialName = null)
        {
            List<PathInventory2> list = new List<PathInventory2>();

            partialName ??= dirInfo.FullName;

            filenameBlacklist ??= new List<string>();

            HashSet<string>? filenameBlacklistHashSet = null;
            if (filenameBlacklist != null)
            {
                filenameBlacklistHashSet = new HashSet<string>(filenameBlacklist, StringComparer.OrdinalIgnoreCase);
            }

            try
            {
                // Asynchronously add all files in the current directory to the list.
                var files = dirInfo.EnumerateFiles();
                foreach (var fileInfo in files)
                {
                    if (filenameBlacklistHashSet != null)
                    {
                        if (filenameBlacklistHashSet.Contains(fileInfo.Name))
                            continue; // Skip files that are in the blacklist
                    }

                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation

                        uint? crc32Hash = null;
                        crc32Hash = await _hashService.ComputeCrc32Async(fileInfo.FullName, cancellationToken);
                        

                        list.Add(new PathInventory2
                        {
                            FullName = fileInfo.FullName,
                            PartialName = fileInfo.FullName.Substring(partialName.Length),
                            Name = fileInfo.Name,
                            RootDir = partialName,
                            Extension = fileInfo.Extension,
                            IsDirectory = false,
                            IsFile = true,
                            Length = fileInfo.Length,
                            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                            EntryDate = DateTime.UtcNow,
                            FileCrc32 = crc32Hash,
                            isOk = true
                        });
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation was canceled. Partial results will be returned.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {fileInfo.FullName}: {ex.Message}");
                        // Continue processing other files despite the error
                        list.Add(new PathInventory2
                        {
                            FullName = fileInfo.FullName,
                            Name = fileInfo.Name,
                            PartialName = fileInfo.FullName.Substring(partialName.Length),
                            RootDir = partialName,
                            Extension = fileInfo.Extension,
                            IsDirectory = false,
                            IsFile = true,
                            Length = fileInfo.Length,
                            LastWriteTimeUtc = fileInfo.LastWriteTimeUtc,
                            EntryDate = DateTime.UtcNow,
                            FileCrc32 = null,
                            isOk = false,
                            ExceptionMessage = ex.Message
                        }); ;
                    }

                    // Introduce a delay every 5000 entries to avoid overwhelming resources
                    if (list.Count % 5000 == 0)
                    {
                        await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
                    }
                }

                // Asynchronously add directories and recurse.
                var directories = dirInfo.EnumerateDirectories();
                foreach (var subDirInfo in directories)
                {
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
                        list.Add(new PathInventory2
                        {
                            FullName = subDirInfo.FullName,
                            PartialName = subDirInfo.FullName.Substring(partialName.Length),
                            Name = subDirInfo.Name,
                            RootDir = partialName,
                            Extension = subDirInfo.Extension,
                            IsDirectory = true,
                            IsFile = false,
                            LastWriteTimeUtc = subDirInfo.LastWriteTimeUtc,
                            EntryDate = DateTime.UtcNow,
                            Length = 0,
                            FileCrc32 = null,
                            isOk = true
                        }); ;

                        var recursiveList = await StripDownGetRecursivePathInventoryAsync2(subDirInfo, cancellationToken, filenameBlacklist,  partialName); // Recursive async call
                        list.AddRange(recursiveList); // Combine results from the recursion
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("Operation was canceled. Partial results will be returned.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing directory {subDirInfo.FullName}: {ex.Message}");
                        // Continue processing other directories despite the error
                    }

                    // Delay again every 5000 entries
                    if (list.Count % 5000 == 0)
                    {
                        await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled. Partial results will be returned.");
            }
            catch (Exception ex)
            {
                // Log other errors as needed
                Console.WriteLine($"Error processing: {ex.Message}");
                list.Add(new PathInventory2
                {
                    FullName = dirInfo.FullName,
                    PartialName = dirInfo.FullName.Substring(partialName.Length),
                    Name = dirInfo.Name,
                    RootDir = partialName,
                    Extension = dirInfo.Extension,
                    IsDirectory = true,
                    IsFile = false,
                    LastWriteTimeUtc = dirInfo.LastWriteTimeUtc,
                    EntryDate = DateTime.UtcNow,
                    Length = 0,
                    isOk = false,
                    FileCrc32 = null,
                    ExceptionMessage = ex.Message
                }); ;
            }
            return list; // Return the partial list gathered until the point of cancellation or complete list if not canceled
        }

    }
}
