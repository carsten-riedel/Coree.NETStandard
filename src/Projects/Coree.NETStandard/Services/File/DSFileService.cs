using Coree.NETStandard.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.File
{
    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    public partial class FileService : ScopedDependencyInjectionSingleton<FileService>, IFileService, IScopedDependencyInjectionSingleton
    {
        /// <summary>
        /// Initializes a new instance of the FileService class with the specified logger and configuration.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        /// <param name="configuration">The configuration instance for accessing application settings.</param>
        public FileService(ILogger<FileService> logger, IConfiguration configuration,IHostEnvironment hostEnvironment) : base(logger, configuration, hostEnvironment) { }
       
    }
}
