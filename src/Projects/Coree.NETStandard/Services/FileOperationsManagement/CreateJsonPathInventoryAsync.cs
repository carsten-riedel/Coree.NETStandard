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
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService, HashService>, IFileOperationsService
    {
 
        public async Task CreateJsonPathInventoryAsync(string? path, string inventoryFilename = "")
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(60000);

            var blacklist = new List<string>() { inventoryFilename };

            if (directoryInfo.Exists)
            {
                var ss = await GetRecursivePathInventoryAsync(directoryInfo, cancellationTokenSource.Token, blacklist, FileVersionHash.None, false);
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

            List<PathInventory> targetlist = await GetRecursivePathInventoryAsync(new DirectoryInfo(target), cancellationTokenSource.Token, null, FileVersionHash.None, false);
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
                            await RetryVerifyAndResumeFileCopyAsync(result[i].FullName, @$"{target}{result[i].PartialName}");
                        }
                        else
                        {
                            if (targetlist[targetEntry].Length != result[i].Length || targetlist[targetEntry].LastWriteTimeUtc != result[i].LastWriteTimeUtc)
                            {
                                await RetryVerifyAndResumeFileCopyAsync(result[i].FullName, @$"{target}{result[i].PartialName}");
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
                _logger?.LogWarning("Extra item, {targetdirectoy} contains {target} that is in {inventoryFilename} but not present in {location}. ", target, item.FullName, inventoryFilename, result.FirstOrDefault()?.RootDir);
                _logger?.LogWarning("{inventoryFilename} is outdated ",inventoryFilename);
            }

            foreach (var item in targetlist)
            {
                
                if (item.IsFile)
                {
                    DeleteFile(item.FullName);
                }

                if (item.IsDirectory)
                {
                    System.IO.Directory.Delete(item.FullName, true);
                }
            }
        }
    }
}

