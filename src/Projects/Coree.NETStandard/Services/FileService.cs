using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services
{
    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    internal interface IFileService
    {
        string? GetCorrectCasedPath(string? path);
        string? IsCommandAvailable(string? command);
    }

    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    public class FileService : IFileService
    {

        private readonly ILogger<FileService> logger;

        /// <summary>
        /// Defines a service for file system operations.
        /// </summary>
        public FileService(ILogger<FileService> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Corrects the casing of a specified file or directory path based on the actual casing on the file system.
        /// </summary>
        /// <remarks>
        /// This method checks the existence of the path and then corrects its casing to match the case used in the file system. 
        /// It considers the file system's case sensitivity: on NTFS or FAT (case-insensitive file systems), it corrects the path casing;
        /// otherwise, it assumes a case-sensitive file system and returns the original path if the drive format is not recognized.
        /// The method processes each component of the path to ensure the entire path is correctly cased from root to leaf.
        /// </remarks>
        /// <param name="path">The path to correct. Can be either a file or directory path.</param>
        /// <returns>The path with corrected casing if the file or directory exists. Returns null for null or empty input,
        /// returns the original path if the path does not exist or if the file system is case-sensitive and not NTFS or FAT.</returns>
        public string? GetCorrectCasedPath(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null; // Handle null or empty input
            }

            var fileInfo = new FileInfo(path);
            var dirInfo = new DirectoryInfo(path);

            if (!fileInfo.Exists && !dirInfo.Exists)
            {
                return null; // The path does not exist; return as-is or handle as needed.
            }

            // Find the drive information for the path
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => path.StartsWith(d.Name, StringComparison.OrdinalIgnoreCase));


            // Check if the drive format is NTFS or FAT; if not, return the original path (assuming case sensitivity)
            if (driveInfo == null)
            {
                return path; // Unable to determine the drive information, return path as-is.
            }

            var checkNTFS = driveInfo.DriveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase);
            var checkFAT = driveInfo.DriveFormat.StartsWith("FAT", StringComparison.OrdinalIgnoreCase);

            if (!(checkFAT || checkNTFS))
            {
                // The drive format is neither NTFS nor FAT, assume case sensitivity and return the path as-is.
                return path;
            }


            var root = Path.GetPathRoot(path);
            if (path.Equals(root, StringComparison.OrdinalIgnoreCase))
            {
                // The path is a root directory (e.g., "C:\")
                return root.ToUpper(); // Or return as is, depending on preference.
            }

            // Split the path into parts (directories + possibly a file)
            var parts = path.Substring(root.Length).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var currentPath = root;
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue; // Skip empty parts

                if (Directory.Exists(currentPath))
                {
                    var directoryInfo = new DirectoryInfo(currentPath);
                    var childInfo = directoryInfo.GetFileSystemInfos(part).FirstOrDefault();
                    if (childInfo != null)
                    {
                        currentPath = Path.Combine(currentPath, childInfo.Name);
                    }
                    else
                    {
                        currentPath = Path.Combine(currentPath, part);
                    }
                }
                else
                {
                    // This could happen if the last part is a file that does not exist
                    currentPath = Path.Combine(currentPath, part);
                    break;
                }
            }

            return currentPath;
        }

        public string? IsCommandAvailable(string? command)
        {
            if (command == null)
            {
                return null;
            }

            var pathVariable2 = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            if (pathVariable2 == null)
            {
                logger.LogWarning("The PATH environment variable is not set or cannot be accessed. Unable to search for command availability.");
                return null;
            }

            string[] pathDirectoryList = pathVariable2.Split(new char[] { Path.PathSeparator },StringSplitOptions.RemoveEmptyEntries);

            //Remove not existing
            for (int i = 0; i < pathDirectoryList.Length; i++)
            {
                var directoryInfo = new System.IO.DirectoryInfo(pathDirectoryList[i]);
                if (directoryInfo.Exists == false)
                {
                    logger.LogInformation($"The PATH contains a directory entry {directoryInfo.FullName} that do not exist, skipping.");
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
                        var directoryInfo = new System.IO.DirectoryInfo(pathDirectoryList[i]);
                        var parent = System.IO.Directory.GetDirectories(directoryInfo.Parent.FullName);
                        var sss = parent.FirstOrDefault(e => e.Equals(pathDirectoryList[i].Trim(Path.DirectorySeparatorChar), StringComparison.InvariantCultureIgnoreCase));

                        pathDirectoryList[i] = sss;
                    }
                    catch (Exception ex)
                    {
                        logger.LogInformation($"The PATH contains a invalid entry skipping. {ex.Message}", ex);
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

        public string? IsExecutableFilePresent(string? command, string path)
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
                System.IO.FileInfo fileinfo = new FileInfo(commandPath);
                if (fileinfo.Exists)
                {
                    var fi = System.IO.Directory.GetFiles(fileinfo.DirectoryName);
                    var re = fi.FirstOrDefault(e => e.Equals(fileinfo.FullName, StringComparison.CurrentCultureIgnoreCase));
                    return re;
                }
            }

            return null;
        }
    }
}

