using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {
        [Flags]
        public enum EntryAttributes
        {
            None = 0,
            ReadOnly = 1 << 0,
            Hidden = 1 << 1,
            System = 1 << 2,
            Directory = 1 << 3,
            Archive = 1 << 4,
            Device = 1 << 5,
            File = 1 << 6,
            Temporary = 1 << 7,
            SparseFile = 1 << 8,
            ReparsePoint = 1 << 9,
            Compressed = 1 << 10,
            Offline = 1 << 11,
            NotContentIndexed = 1 << 12,
            Encrypted = 1 << 13,
            IntegrityStream = 1 << 14,
            Virtual = 1 << 15,
            NoScrubData = 1 << 16
        }

        public class FileSystemEntry
        {
            public string? FullName { get; set; }
            public string? Name { get; set; }
            public EntryAttributes Attributes { get; set; } = EntryAttributes.None;
            public uint? Crc32 { get; set; }
            public long? Length { get; set; }
            public DateTimeOffset LastWriteTimeUtc { get; set; }
            public DateTimeOffset EntryDateUtc { get; set; }

            public Exception? Exception { get; set; }

            [JsonIgnore]
            public bool IsFile
            {
                get
                {
                    // Check if the File flag is set in Attributes
                    return Attributes.HasFlag(EntryAttributes.File);
                }
            }

            [JsonIgnore]
            public bool IsDirectory
            {
                get
                {
                    // Check if the Directory flag is set in Attributes
                    return Attributes.HasFlag(EntryAttributes.Directory);
                }
            }

            [JsonIgnore]
            public string? Extension
            {
                get
                {
                    // Simplify the extension getter since we already determine file status
                    if (IsFile && FullName != null)
                    {
                        return System.IO.Path.GetExtension(FullName);
                    }
                    return null;
                }
            }
        }

        public class FileSystemInformation
        {
            public string Path { get; set; }
            public DateTimeOffset CreatedDateUtc { get; set; } = DateTime.UtcNow;
            public List<FileSystemEntry> FileSystemEntries { get; set; } = new List<FileSystemEntry>();

            [JsonIgnore]
            public List<FileSystemEntry> Ok
            {
                get
                {
                    return FileSystemEntries.Where(e => e.Exception == null).ToList();
                }
            }

            [JsonIgnore]
            public List<FileSystemEntry> Errors
            {
                get
                {
                    return FileSystemEntries.Where(e => e.Exception != null && e.Exception is not OperationCanceledException).ToList();
                }
            }

            [JsonIgnore]
            public List<FileSystemEntry> Canceled
            {
                get
                {
                    return FileSystemEntries.Where(e=>e.Exception is OperationCanceledException).ToList();
                }
            }

            [JsonIgnore]
            public bool HasErrors
            {
                get
                {
                    return Errors.Count > 0;
                }
            }
        }

        public async Task<FileSystemInformation> ScanFileSystemEntriesAsync(string path, CancellationToken cancellationToken, bool failFast = false, bool crc32 = false)
        {
            List<FileSystemEntry> entries = new List<FileSystemEntry>();
            CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            await ScanFileSystemEntriesAsync(path, cancellationTokenSource, failFast, crc32, path, entries);
            return new FileSystemInformation() { Path = path, FileSystemEntries = entries };
        }

        private async Task ScanFileSystemEntriesAsync(string basePath, CancellationTokenSource cancellationTokenSource, bool failFast = false, bool crc32 = false, string? path = null, List<FileSystemEntry>? entries = null)
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
                    Attributes = EntryAttributes.Directory,
                });
                return;
            }
            catch (Exception ex)
            {
                entries.Add(new FileSystemEntry()
                {
                    FullName = path.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                    Exception = ex,
                    Attributes = EntryAttributes.Directory,
                });
                if (failFast) { cancellationTokenSource.Cancel(); return; }
            }

            foreach (var item in dirs)
            {
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();

                    await ScanFileSystemEntriesAsync(basePath, cancellationTokenSource, failFast, crc32, item, entries);

                    var directoryInfo = new DirectoryInfo(item);
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = directoryInfo.FullName.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Attributes = EntryAttributes.Directory,
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
                        Attributes = EntryAttributes.Directory,
                    });
                    return;
                }
                catch (Exception ex)
                {
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = item.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Exception = ex,
                        Attributes = EntryAttributes.Directory,
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

                    uint? crc32Hash = null;
                    if (crc32)
                    {
                        crc32Hash = await _hashService.ComputeCrc32Async(fileInfo.FullName, cancellationTokenSource.Token);
                    }

                    entries.Add(new FileSystemEntry()
                    {
                        FullName = fileInfo.FullName.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Attributes = EntryAttributes.File,
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
                        Attributes = EntryAttributes.File,
                    });
                    return;
                }
                catch (Exception ex)
                {
                    entries.Add(new FileSystemEntry()
                    {
                        FullName = item.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar),
                        Exception = ex,
                        Attributes = EntryAttributes.File,
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