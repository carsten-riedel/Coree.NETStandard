using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.RuntimeInsights
{
    /// <summary>
    /// Represents a service for RuntimeInsightsService methods with optional dependency injection support.
    /// This service implements the IRuntimeInsightsService interface, providing methods reflection of the running system and environment.
    /// This service inherits from DependencySingleton&lt;PInvokeService&gt;, which supports both dependency injection (DI) and non-DI scenarios
    /// </summary>
    public partial class RuntimeInsightsService : DependencySingleton<RuntimeInsightsService>, IRuntimeInsightsService
    {
        /// <summary>
        /// Initializes a new instance of the untimeInsightsService class with the specified logger and configuration.
        /// </summary>
        /// <param name="logger">The logger instance for logging messages.</param>
        /// <param name="configuration">The configuration instance for accessing application settings.</param>
        public RuntimeInsightsService(ILogger<RuntimeInsightsService> logger, IConfiguration configuration) : base(logger, configuration) { }
    }
}
