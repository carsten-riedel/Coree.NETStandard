using System;
using System.Collections.Generic;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.NetworkManagement
{
    /// <summary>
    /// Provides network management services to manage and perform network-related operations.
    /// </summary>
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {
        private readonly ILogger<NetworkService>? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkService"/> with optional logging.
        /// </summary>
        /// <param name="logger">The logger used to log service activity and errors, if any.</param>
        /// <remarks>
        /// Logging can help with diagnosing issues in network operations and service management.
        /// If no logger is provided, the service will operate without logging.
        /// </remarks>
        public NetworkService(ILogger<NetworkService>? logger = null)
        {
            this._logger = logger;
        }
    }

    /// <summary>
    /// Defines the interface for a service that handles network management operations.
    /// </summary>
    public interface INetworkService
    {
        void doo();
    }

}
