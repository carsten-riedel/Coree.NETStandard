using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using Coree.NETStandard.Abstractions.ServiceFactory;
using Microsoft.Extensions.Logging;
using Coree.NETStandard.Services.HashManagement;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {
        public async Task CreateJsonPathInventoryAsync(string? path, string inventoryFilename = "")
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(60000);

            var blacklist = new List<string>() { System.IO.Path.GetFileName(inventoryFilename) };

            if (directoryInfo.Exists)
            {
                FileSystemInformation ss = await ScanFileSystemEntriesAsync(directoryInfo.FullName, cancellationTokenSource.Token, false, false, blacklist);
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

        //public async Task InventoryCopyAsync2(string inventoryFilename, string target)
        //{
        //    if (!System.IO.Directory.Exists(target))
        //    {
        //        System.IO.Directory.CreateDirectory(target);
        //    }

        //    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        //    List<Entry>? result = System.Text.Json.JsonSerializer.Deserialize<List<Entry>>(System.IO.File.ReadAllText(inventoryFilename));
        //    result = result.OrderBy(e => e.IsFile).ToList();

        //    List<Entry> targetlist = await EvolutionPathInventoryFullAsync(target, cancellationTokenSource.Token,false, true);;
        //    targetlist = targetlist.OrderByDescending(e => e.IsFile).ToList();

        //    for (int i = 0; i < result.Count; i++)
        //    {
        //        var targetEntry = targetlist.FindIndex(e => e.FullName == result[i].FullName);

        //        if (result[i].IsDirectory)
        //        {
        //            if (System.IO.Directory.Exists(result[i].FullName))
        //            {
        //                if (!System.IO.Directory.Exists(@$"{target}{result[i].FullName}"))
        //                {
        //                    System.IO.Directory.CreateDirectory(@$"{target}{result[i].FullName}");
        //                }
        //            }
        //            else
        //            {
        //                result.RemoveAt(i);
        //                i--;
        //                continue;
        //            }
        //        }

        //        if (result[i].IsFile)
        //        {
        //            if (System.IO.File.Exists(result[i].FullName))
        //            {
        //                if (targetEntry == -1)
        //                {
        //                    await RetryVerifyAndResumeFileCopyAsync(result[i].FullName, @$"{target}{result[i].FullName}");
        //                }
        //                else
        //                {
        //                    if (targetlist[targetEntry].Length != result[i].Length || targetlist[targetEntry].LastWriteTimeUtc != result[i].LastWriteTimeUtc)
        //                    {
        //                        await RetryVerifyAndResumeFileCopyAsync(result[i].FullName, @$"{target}{result[i].FullName}");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                result.RemoveAt(i);
        //                i--;
        //                continue;
        //            }
        //        }

        //        if (targetEntry != -1)
        //        {
        //            targetlist.RemoveAt(targetEntry);
        //        }
        //    }

        //    targetlist = targetlist.OrderByDescending(e => e.FullName.Length).ToList();

        //    foreach (var item in targetlist)
        //    {
        //        //_logger?.LogWarning("Extra item, {targetdirectoy} contains {target} that is in {inventoryFilename} but not present in {location}. ", target, item.FullName, inventoryFilename, result.FirstOrDefault()?.RootDir);
        //        _logger?.LogWarning("{inventoryFilename} is outdated ",inventoryFilename);
        //    }

        //    foreach (var item in targetlist)
        //    {
        //        if (item.IsFile)
        //        {
        //            DeleteFile(item.FullName);
        //        }

        //        if (item.IsDirectory)
        //        {
        //            System.IO.Directory.Delete(item.FullName, true);
        //        }
        //    }
        //}
    }
}