using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactory;

namespace Coree.NETStandard.Services.FileManagement
{
    public partial class FileService : ServiceFactoryEx<FileService>, IFileService
    {
        /// <summary>
        /// Checks if an executable file with the specified command name is present in the given directory path, considering platform-specific executable extensions.
        /// </summary>
        /// <param name="command">The base name of the executable file to search for, without extension.</param>
        /// <param name="path">The directory path where to look for the executable file.</param>
        /// <returns>The full path of the executable file if found, otherwise null. The method returns null if the directory does not exist or the parameters are null or empty.</returns>
        /// <remarks>
        /// This method supports different sets of executable extensions based on the operating system:
        /// - Windows: Includes common executable and script extensions such as .exe, .bat, .cmd, .ps1, .msi, .vbs, .com, and .scr.
        /// - Linux/macOS: Includes executable and script extensions like .sh, .bash, .run, .bin, as well as scripting languages such as .py, .pl, and .rb.
        /// Files without extensions are also considered in Unix/Linux environments where executables often do not have an extension.
        /// The method checks if the specified directory exists before attempting to find executables, improving efficiency by avoiding unnecessary file operations.
        /// </remarks>
        /// <example>
        /// <code>
        /// string commandName = "myapp";
        /// string directoryPath = "/usr/local/bin";
        /// string result = IsExecutableFilePresent(commandName, directoryPath);
        /// if (result != null)
        /// {
        ///     Console.WriteLine("Executable found: " + result);
        /// }
        /// else
        /// {
        ///     Console.WriteLine("No executable found.");
        /// }
        /// </code>
        /// </example>
        public string? IsExecutableFilePresent(string? command, string? path)
        {
            if (string.IsNullOrEmpty(command) || string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            string[] executableExtensions;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                executableExtensions = new string[] { ".exe", ".bat", ".cmd", ".ps1", ".msi", ".vbs", ".com", ".scr" };
            }
            else
            {
                executableExtensions = new string[] { "", ".sh", ".bash", ".run", ".bin", ".py", ".pl", ".rb" };
            }

            
            foreach (var ext in executableExtensions)
            {
                string commandPath = Path.Combine(path, command + ext);
                if (File.Exists(commandPath))
                {
                    return commandPath;
                }
            }

            return null;
        }
    }
}