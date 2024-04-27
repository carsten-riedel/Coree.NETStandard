using Coree.NETStandard.Abstractions.DependencySingleton;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.Process
{
    /// <summary>
    /// Defines a service for running external processes with support for cancellation and timeouts.
    /// </summary>
    public partial class ProcessService : DependencySingleton<ProcessService>, IProcessService, IDependencySingleton
    {
        /// <summary>
        /// Initializes a new instance of the ProcessService class with the specified logger and configuration.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        /// <param name="configuration">The configuration instance for accessing application settings.</param>
        public ProcessService(ILogger<ProcessService> logger, IConfiguration configuration) : base(logger, configuration) { }
    }
}