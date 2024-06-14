using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileManagement
{
    public partial class FileService : ServiceFactoryEx<FileService>, IFileService
    {
        /// <summary>
        /// Attempts to correct the casing of the provided file or directory path by traversing each segment and matching it against actual filesystem entries.
        /// </summary>
        /// <param name="path">The file or directory path to correct.</param>
        /// <returns>The path with corrected casing for each existing segment. For segments that do not correspond to existing entries, the original casing is used.</returns>
        /// <remarks>
        /// This method operates by:
        /// - Resolving the full path and starting with the root, attempting to correct its casing.
        /// - Sequentially processing each subsequent segment of the path. Each segment's casing is corrected to match the actual filesystem entry if it exists.
        /// - If a segment (directory or file) does not exist, the segment from the original path is used as is, and traversal stops at this point.
        /// This approach ensures that the returned path is as accurate as possible up to the last existing segment. Errors and non-existing segments are handled gracefully by reverting to the original input for those segments.
        /// Exceptional conditions are logged for diagnostic purposes.
        /// </remarks>
        /// <example>
        /// <code>
        /// string originalPath = "c:\\users\\Public\\DESKTOP\\nonExistingFile.txt";
        /// string correctedPath = TryFixPathCaseing(originalPath);
        /// Console.WriteLine(correctedPath); // Output might be "C:\\Users\\Public\\Desktop\\nonExistingFile.txt"
        /// </code>
        /// </example>
        public string? TryFixPathCaseing(string? path)
        {
            if (path == null || string.IsNullOrEmpty(path))
            {
                _logger?.LogDebug("Path is null or empty.");
                return null;
            }

            try
            {
                var fullPath = Path.GetFullPath(path);
                var rootPath = TryCorrectDrivePathCase(Path.GetPathRoot(fullPath));

                if (fullPath.Equals(rootPath, StringComparison.OrdinalIgnoreCase) || rootPath == null || rootPath == string.Empty)
                {
                    return rootPath;
                }

                // Split the path into parts (directories + possibly a file)
                var parts = fullPath.Substring(rootPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                var currentPath = rootPath;

                foreach (var part in parts)
                {
                    if (Directory.Exists(currentPath))
                    {
                        var directoryInfo = new DirectoryInfo(currentPath);
                        var childInfo = directoryInfo.GetFileSystemInfos(part);
                        if (childInfo.Length == 1)
                        {
                            currentPath = Path.Combine(currentPath, childInfo[0].Name);
                        }
                        else if (childInfo.Length > 1)
                        {
                            var exactMatch = childInfo.Where(e => e.Name == part).FirstOrDefault();
                            if (exactMatch != null)
                            {
                                currentPath = Path.Combine(currentPath, exactMatch.Name);
                            }
                            else
                            {
                                currentPath = Path.Combine(currentPath, part);
                            }
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
            catch (Exception ex)
            {
                _logger?.LogDebug("Error processing path: {0}", ex.Message);
                return path;  // Return the original on error
            }
        }

    }
}
