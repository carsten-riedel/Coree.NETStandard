using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Abstractions.DependencySingleton
{
    /// <summary>
    /// Represents an abstract base class for creating thread-safe singleton instances with optional dependency injection support.
    /// This pattern can be utilized in both dependency injection (DI) and non-DI scenarios.
    /// Example usage:
    /// </summary>
    public interface IDependencySingleton
    {
        /// <summary>
        /// Sets the minimum log level filter for the application. This level acts as a filter for the logs that are emitted.
        /// Logs below this level will not be emitted. Default is Information
        /// </summary>
        /// <param name="logLevel">The log level to set as the minimum threshold for logging.</param>
        public void SetLogLevelFilter(LogLevel logLevel = LogLevel.Information);
    }
}
