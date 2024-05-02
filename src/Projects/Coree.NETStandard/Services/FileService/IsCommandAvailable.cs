using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileService
{
    public partial class FileService : ServiceFactory<FileService>, IFileService
    {
        public string? IsCommandAvailable(string? path)
        {
            return IsCommandAvailableAsync(path, new CancellationTokenSource().Token).GetAwaiter().GetResult();
        }

        public async Task<string?> IsCommandAvailableAsync(string? command, CancellationToken cancellationToken)
        {
            if (command == null)
            {
                return null;
            }

            var pathVariable2 = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (pathVariable2 == null)
            {
                _logger?.LogWarning("The PATH environment variable is not set or cannot be accessed. Unable to search for command availability.");
                return null;
            }

            string[] pathDirectoryList = pathVariable2.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries);

            //Remove not existing
            for (int i = 0; i < pathDirectoryList.Length; i++)
            {
                var directoryInfo = new DirectoryInfo(pathDirectoryList[i]);
                if (directoryInfo.Exists == false)
                {
                    _logger?.LogDebug($"The PATH contains a directory entry {directoryInfo.FullName} that do not exist, skipping.");
                    pathDirectoryList[i] = "";
                    continue;
                }
            }
            pathDirectoryList = pathDirectoryList.Where(item => !string.IsNullOrEmpty(item)).ToArray();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                for (int i = 0; i < pathDirectoryList.Length; i++)
                {
                    try
                    {
                        var directoryInfo = new DirectoryInfo(pathDirectoryList[i]);
                        var parent = Directory.GetDirectories(directoryInfo.Parent.FullName);
                        var sss = parent.FirstOrDefault(e => e.Equals(pathDirectoryList[i].Trim(Path.DirectorySeparatorChar), StringComparison.InvariantCultureIgnoreCase));

                        pathDirectoryList[i] = sss;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogInformation($"The PATH contains a invalid entry skipping. {ex.Message}", ex);
                    }
                }
                pathDirectoryList = pathDirectoryList.Where(item => !string.IsNullOrEmpty(item)).ToArray().GroupBy(item => item).Select(group => group.First()).ToArray();
            }

            foreach (var path in pathDirectoryList)
            {
                var present = IsExecutableFilePresent(command, path);
                if (present != null)
                {
                    return $"{present}";
                }
            }

            return null;
        }
    }
}