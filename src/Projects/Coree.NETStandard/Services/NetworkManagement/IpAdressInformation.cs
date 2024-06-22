using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;



namespace Coree.NETStandard.Services.NetworkManagement
{
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {

        /// <summary>
        /// Represents detailed information about an IP address associated with a network interface.
        /// </summary>
        /// <remarks>
        /// Stores IP address details including subnet mask, host names, and DNS suffix related to a network interface.
        /// </remarks>
        public class IpAdressInformation
        {
            /// <summary>
            /// Gets or sets the IP address.
            /// </summary>
            public IPAddress? IPAddress { get; set; }

            /// <summary>
            /// Gets or sets the subnet mask associated with the IP address.
            /// </summary>
            public IPAddress? SubnetMask { get; set; }

            /// <summary>
            /// Gets or sets the host name if the IP address is the local host.
            /// </summary>
            public string? LocalhostHostName { get; set; }

            /// <summary>
            /// Gets or sets the DNS host entry name for the IP address.
            /// </summary>
            public string? HostEntryHostName { get; set; }

            /// <summary>
            /// Gets or sets the local host name with the DNS suffix appended, if available.
            /// </summary>
            public string? LocalhostHostNameWithSuffix { get; set; }

            /// <summary>
            /// Gets a value indicating whether the IP address corresponds to localhost.
            /// </summary>
            public string? LocalhostName
            {
                get
                {
                    if (LocalhostHostName != null)
                    {
                        return "localhost";
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            /// <summary>
            /// Gets or sets the DNS suffix associated with the network adapter.
            /// </summary>
            public string? ApdapterDnsSuffix { get; set; }
        }







    }
}
