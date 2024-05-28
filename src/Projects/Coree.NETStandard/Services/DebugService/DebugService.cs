using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.DebugManagement
{
    /// <summary>
    /// Provides debugging services to log messages internally and via a provided logger.
    /// </summary>
    /// <remarks>
    /// This service should be used for development and debugging purposes only.
    /// It is designed to extend easily and integrates with the standard logging infrastructure.
    /// </remarks>
    public class DebugService : ServiceFactoryEx<DebugService>, IDebugService
    {
        private readonly ILogger<DebugService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugService"/> class.
        /// </summary>
        /// <param name="logger">The logger used to log messages.</param>
        /// <remarks>
        /// The constructor writes a debug line to indicate its invocation.
        /// </remarks>
        public DebugService(ILogger<DebugService> logger)
        {
            this.logger = logger;
            Debug.WriteLine("DebugService ctor");
        }

        /// <summary>
        /// Logs a message using the configured logger and writes it to the debug output.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <remarks>
        /// This method provides dual logging through the ILogger interface and System.Diagnostics.Debug.
        /// </remarks>
        public void LogMessage(string message)
        {
            this.logger.LogInformation(message);
            Debug.WriteLine(message);
        }
    }

    /// <summary>
    /// Defines a service for logging debug messages.
    /// </summary>
    public interface IDebugService
    {
        /// <summary>
        /// Logs a message to the logging infrastructure.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void LogMessage(string message);
    }
}
