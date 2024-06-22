using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.NetworkManagement
{
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {
        /// <summary>
        /// Retrieves IP address information for network interfaces that are operational, have DHCP and DNS configured, and include a gateway address.
        /// </summary>
        /// <remarks>
        /// This method selects network interfaces that are in an operational state with configured DHCP, DNS, and gateway addresses, and then gathers all related IP address information.
        /// It filters for IP addresses that have a corresponding host entry hostname, indicating valid DNS resolution.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="IpAdressInformation"/> objects, each containing IP address details for interfaces that meet the specified conditions.
        /// </returns>
        /// <example>
        /// <code>
        /// var ipInfos = GetNetworkIpAdressUpDhcpDnsGateway();
        /// foreach (var ipInfo in ipInfos)
        /// {
        ///     Console.WriteLine($"IP Address: {ipInfo.IPAddress} - Hostname: {ipInfo.HostEntryHostName}");
        /// }
        /// </code>
        /// </example>
        public List<IpAdressInformation> GetNetworkIpAdressUpDhcpDnsGateway()
        {
            var networkInterfaceDetails = GetNetworkInterfaceDetailsUpDhcpDnsGateway();
            return networkInterfaceDetails.SelectMany(e => e.IpAdressHostNames).Where(e => e.HostEntryHostName != null).ToList();
        }
    }
}
