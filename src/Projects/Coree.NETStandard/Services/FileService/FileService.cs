using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileService
{
    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    /// <remarks>
    /// <code>
    /// // Configuring a self-contained ServiceStack setup using Host builder's CreateDefaultBuilder method.
    /// FileService fileServiceService = FileService.CreateServiceFactory();
    /// 
    /// // Instantiating the FileService class directly
    /// FileService fileService = new FileService();
    /// 
    /// // Implementing FileService in a Dependency Injection (DI) scenario
    /// var builder = Host.CreateDefaultBuilder();
    /// builder.ConfigureServices((context, services) =>
    /// {
    ///     services.AddSingleton&gt;IFileService, FileService&lt;();
    /// });
    /// </code>
    /// </remarks>
    public partial class FileService : ServiceFactory<FileService>, IFileService
    {
        private readonly ILogger<FileService>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileService"/> class.
        /// </summary>
        /// <param name="logger">Optional logger instance for logging purposes.</param>
        /// <remarks>
        /// The logger provided here can be used with the field within the class.
        /// Be mindful that the logger may be null in scenarios where it's not explicitly provided.
        /// </remarks>
        public FileService(ILogger<FileService>? logger = null)
        {
            this._logger = logger;
        }
    }

    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    public interface IFileService
    {

        /// <summary>
        /// Checks if a specified command is available in the current directory or any of the directories listed in the system's PATH environment variable.
        /// </summary>
        /// <param name="command">The name of the executable file to search for.</param>
        /// <returns>The full path of the executable file with corrected casing if found; otherwise, returns null. The method returns null if the command parameter is null, or if the executable cannot be found in the current or PATH directories.</returns>
        /// <remarks>
        /// This method performs the following operations:
        /// - Validates the command input.
        /// - Retrieves the current directory and the directories from the system's PATH environment using the GetValidUniquePaths method, which excludes invalid and duplicate paths.
        /// - Checks each directory for the existence of the specified command.
        /// - Utilizes the TryFixPathCaseing method to return the path in the correct casing.
        /// - Logs various informational messages during the process, including errors encountered, invalid path entries, and successful detection of the executable.
        /// This method ensures that the search includes the current directory, aligning with common command-line behaviors where the current directory is typically searched before the PATH directories.
        /// </remarks>
        /// <example>
        /// <code>
        /// string commandName = "example";
        /// string executablePath = IsCommandAvailable(commandName);
        /// if (executablePath != null)
        /// {
        ///     Console.WriteLine($"Executable found: {executablePath}");
        /// }
        /// else
        /// {
        ///     Console.WriteLine("Executable not found.");
        /// }
        /// </code>
        /// </example>
        string? IsCommandAvailable(string? command);

        /// <summary>
        /// Retrieves a list of valid and unique directory paths from the system's PATH environment variable.
        /// </summary>
        /// <returns>A list of strings representing valid, unique, and normalized directory paths from the PATH environment. Returns an empty list if the PATH environment variable is not accessible or if no valid paths are found.</returns>
        /// <remarks>
        /// This method processes each entry in the PATH environment variable by:
        /// - Normalizing the casing of each path using the TryFixPathCaseing method.
        /// - Checking each path for existence to ensure validity.
        /// - Ensuring that each path is unique within the context of the PATH variable to avoid duplicates.
        /// - Logging various statuses such as inaccessible PATH, skipped entries due to invalidity or duplication, and errors during processing.
        /// The method is robust against non-existent directories, permission issues, and other filesystem anomalies by logging and skipping over problematic entries.
        /// </remarks>
        /// <example>
        /// <code>
        /// var validPaths = GetValidUniquePaths();
        /// foreach (var path in validPaths)
        /// {
        ///     Console.WriteLine(path); // Prints each valid and unique path
        /// }
        /// </code>
        /// </example>
        List<string> GetValidUniquePaths();

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
        string? IsExecutableFilePresent(string? command, string? path);

        /// <summary>
        /// Checks whether the specified path is a valid file or directory.
        /// </summary>
        /// <param name="path">The path to be checked.</param>
        /// <returns>
        /// <c>true</c> if the path points to an existing file or directory; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method returns <c>false</c> if the provided path is null or empty.
        /// It logs debug messages using the provided logger instance if any errors occur during the validation process.
        /// </remarks>
        bool IsValidLocation(string? path);

        /// <summary>
        /// Attempts to retrieve the correctly cased drive root path based on a provided drive name, ignoring case sensitivity.
        /// </summary>
        /// <param name="drivename">The drive name to search for, case-insensitively.</param>
        /// <returns>The correctly cased drive root path if found; otherwise, returns the original drivename.</returns>
        /// <remarks>
        /// This method performs a case-insensitive comparison to find a matching drive among the available drives.
        /// If no matching drive is found or if an exception occurs during the drive search, the original drivename is returned.
        /// This ensures that the method fails gracefully, providing a fallback to the original input.
        /// </remarks>
        /// <example>
        /// <code>
        /// string drivePath = TryCorrectDrivePathCase("C:");
        /// Console.WriteLine(drivePath); // Output might be "C:\", or "C:" if no match is found
        /// </code>
        /// </example>
        string? TryCorrectDrivePathCase(string? drivename);

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
        string? TryFixPathCaseing(string? path);

        Task CreateJsonPathInventoryAsync(string? path, string? inventoryFilename = "");

        Task InventoryCopyAsync(string inventoryFilename, string target);
    }
}
