using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.NetworkManagement
{
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {
        /// <summary>
        /// Contains all relevant details of a network interface, including hardware properties and IP configuration.
        /// </summary>
        public class NetworkInterfaceDetail
        {
            /// <summary>
            /// Gets or sets the network interface object representing the hardware and operational status.
            /// </summary>
            public NetworkInterface? NetworkInterface { get; set; }

            /// <summary>
            /// Gets or sets the IP properties of the network interface, providing details like DHCP, DNS settings, and more.
            /// </summary>
            public IPInterfaceProperties? IPInterfaceProperties { get; set; }

            /// <summary>
            /// Gets or sets a list of IP address information objects, each representing detailed IP configuration for the network interface.
            /// </summary>
            public List<IpAdressInformation>? IpAdressHostNames { get; set; }
        }
    }
}
