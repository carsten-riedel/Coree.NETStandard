using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {
        public async Task CreateJsonPathInventoryAsync(string? path, string inventoryFilename = "inventory.json", CancellationToken cancellationToken = default)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            var blacklist = new List<string>() { System.IO.Path.GetFileName(inventoryFilename) };

            if (directoryInfo.Exists)
            {
                FileSystemInformation ss = await ScanFileSystemEntriesAsync(directoryInfo.FullName, cancellationToken, false, false, blacklist);
                var result = System.Text.Json.JsonSerializer.Serialize(ss, new JsonSerializerOptions() { WriteIndented = true });
                System.IO.File.WriteAllText(@$"{Path.Combine(path,inventoryFilename)}", result);
            }
        }

        public async Task InventoryCopyAsync(string inventoryFilename, string target)
        {
            if (!System.IO.Directory.Exists(target))
            {
                System.IO.Directory.CreateDirectory(target);
            }

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            FileSystemInformation? SourceEntrys = System.Text.Json.JsonSerializer.Deserialize<FileSystemInformation>(System.IO.File.ReadAllText(inventoryFilename));
            SourceEntrys.FileSystemEntries = SourceEntrys.FileSystemEntries.OrderBy(e => e.IsFile).ToList();

            for (int i = 0; i < SourceEntrys.FileSystemEntries.Count; i++)
            {
                if (SourceEntrys.FileSystemEntries[i].IsDirectory)
                {
                    if (!System.IO.Directory.Exists(Path.Combine(SourceEntrys.Path, SourceEntrys.FileSystemEntries[i].FullName)))
                    {
                        SourceEntrys.FileSystemEntries.RemoveAt(i);
                        i--;
                    }
                }

                if (SourceEntrys.FileSystemEntries[i].IsFile)
                {
                    if (!System.IO.File.Exists(Path.Combine(SourceEntrys.Path, SourceEntrys.FileSystemEntries[i].FullName)))
                    {
                        SourceEntrys.FileSystemEntries.RemoveAt(i);
                        i--;
                    }
                }
            }

            FileSystemInformation? TargetEntrys = await ScanFileSystemEntriesAsync(target, cancellationTokenSource.Token, false, false); ;
            TargetEntrys.FileSystemEntries = TargetEntrys.FileSystemEntries.OrderByDescending(e => e.IsFile).ToList();

            for (int i = 0; i < SourceEntrys.FileSystemEntries.Count; i++)
            {
                FileSystemEntry sourceEntry = SourceEntrys.FileSystemEntries[i];
                int targetEntryIndex = TargetEntrys.FileSystemEntries.FindIndex(e => e.FullName == sourceEntry.FullName);
                FileSystemEntry? targetEntry = null;
                if (targetEntryIndex != -1)
                {
                    targetEntry = TargetEntrys.FileSystemEntries[targetEntryIndex];
                }

                if (sourceEntry.IsDirectory)
                {
                    if (!(System.IO.Directory.Exists(Path.Combine(TargetEntrys.Path, sourceEntry.FullName))))
                    {
                        System.IO.Directory.CreateDirectory(Path.Combine(TargetEntrys.Path, sourceEntry.FullName));
                    }
                }

                if (sourceEntry.IsFile)
                {
                    if (targetEntry == null)
                    {
                        await RetryVerifyAndResumeFileCopyAsync(Path.Combine(SourceEntrys.Path, sourceEntry.FullName), Path.Combine(TargetEntrys.Path, sourceEntry.FullName));
                    }
                    else
                    {
                        if (targetEntry.Length != sourceEntry.Length || targetEntry.LastWriteTimeUtc != sourceEntry.LastWriteTimeUtc || targetEntry.Crc32 != sourceEntry.Crc32)
                        {
                            await RetryVerifyAndResumeFileCopyAsync(Path.Combine(SourceEntrys.Path, sourceEntry.FullName), Path.Combine(TargetEntrys.Path, sourceEntry.FullName));
                        }
                    }
                }

                if (targetEntryIndex != -1)
                {
                    TargetEntrys.FileSystemEntries.RemoveAt(targetEntryIndex);
                }
            }

            TargetEntrys.FileSystemEntries = TargetEntrys.FileSystemEntries.OrderByDescending(e => e.FullName.Length).ToList();

            foreach (var item in TargetEntrys.FileSystemEntries)
            {
                //_logger?.LogWarning("Extra item, {targetdirectoy} contains {target} that is in {inventoryFilename} but not present in {location}. ", target, item.FullName, inventoryFilename, result.FirstOrDefault()?.RootDir);
                _logger?.LogWarning("{inventoryFilename} is outdated ", inventoryFilename);
            }

            foreach (var item in TargetEntrys.FileSystemEntries)
            {
                if (item.IsFile)
                {
                    
                    DeleteFile(Path.Combine(TargetEntrys.Path, item.FullName));
                }

                if (item.IsDirectory)
                {
                    System.IO.Directory.Delete(Path.Combine(TargetEntrys.Path, item.FullName), true);
                }
            }
        }

    }
}