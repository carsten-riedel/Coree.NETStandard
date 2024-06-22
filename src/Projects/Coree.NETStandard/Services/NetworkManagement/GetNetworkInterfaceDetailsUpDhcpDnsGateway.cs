using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.NetworkManagement
{
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {

        /// <summary>
        /// Retrieves a list of network interfaces that are operational and configured with DHCP, DNS, and gateway addresses.
        /// </summary>
        /// <remarks>
        /// This method filters network interfaces to include only those that are "Up" (operational) and have configured DHCP server addresses, DNS addresses, 
        /// and gateway addresses. It relies on the <see cref="GetNetworkInterfaceDetails"/> method to initially fetch all network interfaces and their details.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="NetworkInterfaceDetail"/> objects, each representing a network interface that meets the specified criteria of being operational with DHCP, DNS, 
        /// and gateway configurations.
        /// </returns>
        /// <example>
        /// <code>
        /// var detailedInterfaces = GetNetworkInterfaceDetailsUpDhcpDnsGateway();
        /// foreach (var interfaceDetail in detailedInterfaces)
        /// {
        ///     Console.WriteLine($"Interface: {interfaceDetail.NetworkInterface.Name}");
        ///     Console.WriteLine($"DHCP: {interfaceDetail.IPInterfaceProperties.DhcpServerAddresses.Count}");
        ///     Console.WriteLine($"DNS: {interfaceDetail.IPInterfaceProperties.DnsAddresses.Count}");
        ///     Console.WriteLine($"Gateway: {interfaceDetail.IPInterfaceProperties.GatewayAddresses.Count}");
        /// }
        /// </code>
        /// </example>
        public List<NetworkInterfaceDetail> GetNetworkInterfaceDetailsUpDhcpDnsGateway()
        {
            var networkInterfaceDetails = GetNetworkInterfaceDetails();
            var networkInterfaceDetailsUp = networkInterfaceDetails.Where(e => e.NetworkInterface?.OperationalStatus == OperationalStatus.Up);
            var networkInterfaceDetailsUpDhcp = networkInterfaceDetailsUp.Where(e => e.IPInterfaceProperties?.DhcpServerAddresses.Count > 0);
            var networkInterfaceDetailsUpDhcpDns = networkInterfaceDetailsUpDhcp.Where(e => e.IPInterfaceProperties?.DnsAddresses.Count > 0);
            var networkInterfaceDetailsUpDhcpDnsGateway = networkInterfaceDetailsUpDhcpDns.Where(e => e.IPInterfaceProperties?.GatewayAddresses.Count > 0);
            return networkInterfaceDetailsUpDhcpDnsGateway.ToList();
        }
    }
}
