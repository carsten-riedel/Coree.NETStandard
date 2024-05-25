using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactory;

namespace Coree.NETStandard.Services.FileService
{
    public partial class FileService : ServiceFactory<FileService>, IFileService
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

        //static async Task AddFileSystemEntriesAsync(string path, List<FileSystemInfo> list)
        //{
        //    try
        //    {
        //        // Asynchronously add all files in the current directory to the list.
        //        var files = Directory.EnumerateFiles(path);
        //        foreach (var filePath in files)
        //        {
        //            FileInfo fileInfo = new FileInfo(filePath);
        //            list.Add(fileInfo);

        //            // Introduce a delay every 5000 entries to avoid overwhelming resources
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100);
        //            }
        //        }

        //        // Asynchronously add directories and recurse.
        //        var directories = Directory.EnumerateDirectories(path);
        //        foreach (var directoryPath in directories)
        //        {
        //            DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
        //            list.Add(dirInfo);

        //            // Delay again every 5000 entries
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100);
        //            }
        //            await AddFileSystemEntriesAsync(directoryPath, list); // Recursive async call
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error or handle it as needed
        //        Console.WriteLine($"Error accessing {path}: {ex.Message}");
        //    }
        //}

        //static async Task AddFileSystemEntriesAsync2(DirectoryInfo dirInfo, List<FileSystemInfo> list)
        //{
        //    try
        //    {
        //        // Asynchronously add all files in the current directory to the list.
        //        var files = dirInfo.EnumerateFiles();
        //        foreach (var fileInfo in files)
        //        {
        //            list.Add(fileInfo);

        //            // Introduce a delay every 5000 entries to avoid overwhelming resources
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100);
        //            }
        //        }

        //        // Asynchronously add directories and recurse.
        //        var directories = dirInfo.EnumerateDirectories();
        //        foreach (var subDirInfo in directories)
        //        {
        //            list.Add(subDirInfo);

        //            // Delay again every 5000 entries
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100);
        //            }
        //            await AddFileSystemEntriesAsync2(subDirInfo, list); // Recursive async call
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error or handle it as needed
        //        Console.WriteLine($"Error accessing {dirInfo.FullName}: {ex.Message}");
        //    }
        //}

        //static async Task<List<FileSystemInfo>> AddFileSystemEntriesAsync3(DirectoryInfo dirInfo)
        //{
        //    List<FileSystemInfo> list = new List<FileSystemInfo>();
        //    try
        //    {
        //        // Asynchronously add all files in the current directory to the list.
        //        var files = dirInfo.EnumerateFiles();
        //        foreach (var fileInfo in files)
        //        {
        //            list.Add(fileInfo);

        //            // Introduce a delay every 5000 entries to avoid overwhelming resources
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100);
        //            }
        //        }

        //        // Asynchronously add directories and recurse.
        //        var directories = dirInfo.EnumerateDirectories();
        //        foreach (var subDirInfo in directories)
        //        {
        //            list.Add(subDirInfo);

        //            // Delay again every 5000 entries
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100);
        //            }
        //            var recursiveList = await AddFileSystemEntriesAsync3(subDirInfo); // Recursive async call
        //            list.AddRange(recursiveList); // Combine results from the recursion
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error or handle it as needed
        //        Console.WriteLine($"Error accessing {dirInfo.FullName}: {ex.Message}");
        //    }
        //    return list;
        //}

        //static async Task<List<FileSystemInfo>> AddFileSystemEntriesAsync4(DirectoryInfo dirInfo, CancellationToken cancellationToken)
        //{
        //    List<FileSystemInfo> list = new List<FileSystemInfo>();
        //    try
        //    {
        //        // Asynchronously add all files in the current directory to the list.
        //        var files = dirInfo.EnumerateFiles();
        //        foreach (var fileInfo in files)
        //        {
        //            cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
        //            list.Add(fileInfo);

        //            // Introduce a delay every 5000 entries to avoid overwhelming resources
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
        //            }
        //        }

        //        // Asynchronously add directories and recurse.
        //        var directories = dirInfo.EnumerateDirectories();
        //        foreach (var subDirInfo in directories)
        //        {
        //            cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
        //            list.Add(subDirInfo);

        //            // Delay again every 5000 entries
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
        //            }
        //            var recursiveList = await AddFileSystemEntriesAsync4(subDirInfo, cancellationToken); // Recursive async call
        //            list.AddRange(recursiveList); // Combine results from the recursion
        //        }
        //    }
        //    catch (Exception ex) when (!(ex is OperationCanceledException))
        //    {
        //        // Log the error or handle it as needed
        //        Console.WriteLine($"Error accessing {dirInfo.FullName}: {ex.Message}");
        //    }
        //    return list;
        //}

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

        //private static async Task<List<PathInventory>> GetPathInventory(DirectoryInfo dirInfo, CancellationToken cancellationToken, bool withFileVersionHash = true, bool withFileHash = true)
        //{
        //    List<PathInventory> list = new List<PathInventory>();
        //    try
        //    {
        //        // Asynchronously add all files in the current directory to the list.
        //        var files = dirInfo.EnumerateFiles();

        //        foreach (var fileInfo in files)
        //        {
        //            try
        //            {
        //                cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
        //                string? versionHash = null;
        //                string? fileHash = null;
        //                if (withFileVersionHash)
        //                {
        //                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileInfo.FullName);
        //                    versionHash = await ComputeMD5StringAsync(fileVersionInfo.ToString(), cancellationToken);
        //                }
        //                if (withFileHash)
        //                {
        //                    fileHash = await ComputeMD5FileAsync(fileInfo.FullName, cancellationToken);
        //                }

        //                list.Add(new PathInventory() { FullName = fileInfo.FullName, Name = fileInfo.Name, Extension = fileInfo.Extension, IsDirectory = false, IsFile = true, Length = fileInfo.Length, LastWriteTimeUtc = fileInfo.LastWriteTimeUtc, FileVersionHash = versionHash, FileHash = fileHash, isOk = true });
        //            }
        //            catch (Exception ex)
        //            {
        //                Console.WriteLine($"Error processing file {fileInfo.FullName}: {ex.Message}");
        //            }

        //            // Introduce a delay every 5000 entries to avoid overwhelming resources
        //            if (list.Count % 5000 == 0)
        //            {
        //                await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
        //            }

        //            // Asynchronously add directories and recurse.
        //            var directories = dirInfo.EnumerateDirectories();
        //            foreach (var subDirInfo in directories)
        //            {
        //                try
        //                {
        //                    cancellationToken.ThrowIfCancellationRequested(); // Check for cancellation
        //                    list.Add(new PathInventory() { FullName = subDirInfo.FullName, Name = subDirInfo.Name, Extension = subDirInfo.Extension, IsDirectory = true, IsFile = false, LastWriteTimeUtc = subDirInfo.LastWriteTimeUtc, Length = 0, isOk = true });
        //                    var recursiveList = await GetPathInventory(subDirInfo, cancellationToken, withFileVersionHash, withFileHash); // Recursive async call
        //                    list.AddRange(recursiveList); // Combine results from the recursion
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Error processing directory {subDirInfo.FullName}: {ex.Message}");
        //                    // Continue processing other directories despite the error
        //                }

        //                // Delay again every 5000 entries
        //                if (list.Count % 5000 == 0)
        //                {
        //                    await Task.Delay(100, cancellationToken); // Pass cancellation token to Task.Delay
        //                }
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        Console.WriteLine("Operation was canceled. Partial results will be returned.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log other errors as needed
        //        Console.WriteLine($"Error accessing {dirInfo.FullName}: {ex.Message}");
        //    }
        //    return list; // Return the partial list gathered until the point of cancellation or complete list if not canceled
        //}

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

        private static async Task<List<PathInventory>> GetRecursivePathInventoryAsync(DirectoryInfo dirInfo, CancellationToken cancellationToken, List<string>? filenameBlacklist = null, FileVersionHash withFileVersionHash = FileVersionHash.None, bool withFileHash = true, string? partialName = null)
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

        public async Task CreateJsonPathInventoryAsync(string? path, string inventoryFilename = "")
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(60000);

            var blacklist = new List<string>() { inventoryFilename };

            if (directoryInfo.Exists)
            {
                var ss = await GetRecursivePathInventoryAsync(directoryInfo, cancellationTokenSource.Token, blacklist, FileVersionHash.None, false);
                var testx = ss.Where(e => e.Extension == "" && e.IsFile == true);
                var test = ss.Where(e => e.isOk == false);
                var testa = ss.OrderByDescending(e => e.Length);

                var result = System.Text.Json.JsonSerializer.Serialize(ss, new JsonSerializerOptions() { WriteIndented = true });
                System.IO.File.WriteAllText(@$"{path}\{inventoryFilename}", result);
            }
        }

        public async Task InventoryCopyAsync(string inventoryFilename, string target)
        {
            if (!System.IO.Directory.Exists(target))
            {
                System.IO.Directory.CreateDirectory(target);
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            List<PathInventory>? result = System.Text.Json.JsonSerializer.Deserialize<List<PathInventory>>(System.IO.File.ReadAllText(inventoryFilename));
            result = result.OrderBy(e => e.IsFile).ToList();

            var targetlist = await GetRecursivePathInventoryAsync(new DirectoryInfo(target), cancellationTokenSource.Token, null, FileVersionHash.None, false);
            targetlist = targetlist.OrderByDescending(e => e.IsFile).ToList();

            for (int i = 0; i < result.Count; i++)
            {
                var targetEntry = targetlist.FindIndex(e => e.PartialName == result[i].PartialName);
       

                if (result[i].IsDirectory)
                {
                    if (System.IO.Directory.Exists(result[i].FullName))
                    {
                        if (!System.IO.Directory.Exists(@$"{target}{result[i].PartialName}"))
                        {
                            System.IO.Directory.CreateDirectory(@$"{target}{result[i].PartialName}");
                        }
                    }
                    else
                    {
                        result.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                if (result[i].IsFile)
                {
                    if (System.IO.File.Exists(result[i].FullName))
                    {
                        if (targetEntry == -1)
                        {
                            System.IO.File.Copy(result[i].FullName, @$"{target}{result[i].PartialName}", true);
                        }
                        else
                        {
                            if (targetlist[targetEntry].Length != result[i].Length || targetlist[targetEntry].LastWriteTimeUtc != result[i].LastWriteTimeUtc)
                            {
                                System.IO.File.Copy(result[i].FullName, @$"{target}{result[i].PartialName}", true);
                            }
                        }
                    }
                    else
                    {
                        result.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                
                if (targetEntry != -1)
                {
                    targetlist.RemoveAt(targetEntry);
                }
            }


            targetlist = targetlist.OrderByDescending(e => e.FullName.Length).ToList();

            foreach (var item in targetlist)
            {
                if (item.IsFile)
                {
                    System.IO.File.Delete(item.FullName);
                }

                if (item.IsDirectory)
                {
                    System.IO.Directory.Delete(item.FullName, true);
                }
            }
        }
    }
}