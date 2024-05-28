using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.DirectoryOperationsManagement
{
    /// <summary>
    /// Provides services for performing operations on directories.
    /// </summary>
    /// <remarks>
    /// This class can be used to ensure the existence of directories, potentially logging actions depending on configuration.
    /// </remarks>
    public partial class DirectoryOperationsService : ServiceFactoryEx<DirectoryOperationsService>, IDirectoryOperationsService
    {
        private readonly ILogger<DirectoryOperationsService>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryOperationsService"/> class.
        /// </summary>
        /// <param name="logger">Optional. The logger used for logging operations within the service.</param>
        /// <remarks>
        /// If a logger is not provided, logging will not be performed.
        /// </remarks>
        public DirectoryOperationsService(ILogger<DirectoryOperationsService>? logger = null)
        {
            this._logger = logger;
        }
    }

    /// <summary>
    /// Defines a service for managing directory operations.
    /// </summary>
    public interface IDirectoryOperationsService
    {
        /// <summary>
        /// Ensures that a specified directory exists, creating it if necessary.
        /// </summary>
        /// <param name="directory">The path of the directory to check and create if necessary.</param>
        /// <returns>true if the directory exists or was successfully created; otherwise, false.</returns>
        bool EnsureDirectory(string directory);
    }
}
