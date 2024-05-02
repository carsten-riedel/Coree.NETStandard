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

        string? IsCommandAvailable(string? command);

        Task<string?> IsCommandAvailableAsync(string? command, CancellationToken cancellationToken);

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
    }
}
