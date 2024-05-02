using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactory;

namespace Coree.NETStandard.Services.FileService
{
    public partial class FileService : ServiceFactory<FileService>, IFileService
    {
        public string? IsExecutableFilePresent(string? command, string path)
        {
            return IsExecutableFilePresentAsync(command, path).GetAwaiter().GetResult();
        }

        public async Task<string?> IsExecutableFilePresentAsync(string? command, string path)
        {
            if (command == null)
            {
                return null;
            }

            string[] executableExtensions = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? new string[] { ".exE", ".bat", ".cmd", ".ps1" }
                : new string[] { "", ".sh" }; // No extension for Linux/MacOS

            foreach (var ext in executableExtensions)
            {
                string commandPath = Path.Combine(path, command + ext);
                FileInfo fileinfo = new FileInfo(commandPath);
                if (fileinfo.Exists)
                {
                    var fi = Directory.GetFiles(fileinfo.DirectoryName);
                    var re = fi.FirstOrDefault(e => e.Equals(fileinfo.FullName, StringComparison.CurrentCultureIgnoreCase));
                    return re;
                }
            }

            return null;
        }
    }
}