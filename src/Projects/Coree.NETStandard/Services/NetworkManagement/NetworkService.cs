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
        /// <summary>
        /// Retrieves detailed information about network interfaces on the local machine, including IP address details and DNS suffixes.
        /// </summary>
        /// <remarks>
        /// This method identifies all network interfaces available on the local machine and gathers extensive details such as IP addresses, subnet masks,
        /// DNS eligibility, and DNS suffixes. It handles both IPv4 and IPv6 addresses and includes special handling to identify if an address is associated with the local machine itself.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="NetworkService.NetworkInterfaceDetail"/> objects, each containing details about a single network interface, such as associated IP addresses and their properties.
        /// </returns>
        /// <example>
        /// <code>
        /// var networkDetails = GetNetworkInterfaceDetails();
        /// foreach (var detail in networkDetails)
        /// {
        ///     Console.WriteLine(detail.NetworkInterface.Description);
        ///     foreach (var ipInfo in detail.IpAdressHostNames)
        ///     {
        ///         Console.WriteLine($"IP: {ipInfo.IPAddress}, Subnet: {ipInfo.SubnetMask}");
        ///     }
        /// }
        /// </code>
        /// </example>
        List<NetworkService.NetworkInterfaceDetail> GetNetworkInterfaceDetails();

        /// <summary>
        /// Retrieves a list of network interfaces that are operational and configured with DHCP, DNS, and gateway addresses.
        /// </summary>
        /// <remarks>
        /// This method filters network interfaces to include only those that are "Up" (operational) and have configured DHCP server addresses, DNS addresses, 
        /// and gateway addresses. It relies on the <see cref="GetNetworkInterfaceDetails"/> method to initially fetch all network interfaces and their details.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="NetworkService.NetworkInterfaceDetail"/> objects, each representing a network interface that meets the specified criteria of being operational with DHCP, DNS, 
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
        List<NetworkService.NetworkInterfaceDetail> GetNetworkInterfaceDetailsUpDhcpDnsGateway();

        /// <summary>
        /// Retrieves IP address information for network interfaces that are operational, have DHCP and DNS configured, and include a gateway address.
        /// </summary>
        /// <remarks>
        /// This method selects network interfaces that are in an operational state with configured DHCP, DNS, and gateway addresses, and then gathers all related IP address information.
        /// It filters for IP addresses that have a corresponding host entry hostname, indicating valid DNS resolution.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="NetworkService.IpAdressInformation"/> objects, each containing IP address details for interfaces that meet the specified conditions.
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
        List<NetworkService.IpAdressInformation> GetNetworkIpAdressUpDhcpDnsGateway();
    }
}