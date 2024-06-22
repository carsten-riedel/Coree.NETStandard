using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;
using System.Linq;

namespace Coree.NETStandard.Services.NetworkManagement
{
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {

        /// <summary>
        /// Retrieves detailed information about network interfaces on the local machine, including IP address details and DNS suffixes.
        /// </summary>
        /// <remarks>
        /// This method identifies all network interfaces available on the local machine and gathers extensive details such as IP addresses, subnet masks,
        /// DNS eligibility, and DNS suffixes. It handles both IPv4 and IPv6 addresses and includes special handling to identify if an address is associated with the local machine itself.
        /// </remarks>
        /// <returns>
        /// A list of <see cref="NetworkInterfaceDetail"/> objects, each containing details about a single network interface, such as associated IP addresses and their properties.
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
        public List<NetworkInterfaceDetail> GetNetworkInterfaceDetails()
        {
            var localhostName = Dns.GetHostEntry(string.Empty);
            var localhostNameAddressList = localhostName.AddressList.ToList();
            localhostNameAddressList.Add(IPAddress.Loopback);
            localhostNameAddressList.Add(IPAddress.IPv6Loopback);
            IPAddress[] localhostIPAddress = localhostNameAddressList.ToArray();

            List<NetworkInterfaceDetail> networkInterfaceDetails = new List<NetworkInterfaceDetail>();
            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                UnicastIPAddressInformation[] uniCastIPAddress = ipProps.UnicastAddresses.ToArray();

                List<IpAdressInformation> ipAdressInformations = new List<IpAdressInformation>();
                for (int i = 0; i < uniCastIPAddress.Length; i++)
                {
                    IpAdressInformation ipAdressInformation = new IpAdressInformation();

                    ipAdressInformation.IPAddress = uniCastIPAddress[i].Address;
                    if (ipAdressInformation.IPAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAdressInformation.SubnetMask = uniCastIPAddress[i].IPv4Mask;
                    }

                    if (localhostIPAddress.Contains(uniCastIPAddress[i].Address))
                    {
                        ipAdressInformation.LocalhostHostName = localhostName.HostName;
                    }

                    if (uniCastIPAddress[i].IsDnsEligible == true)
                    {
                        ipAdressInformation.HostEntryHostName = Dns.GetHostEntry(uniCastIPAddress[i].Address).HostName;
                    }

                    if (!string.IsNullOrEmpty(ipProps.DnsSuffix) && !string.IsNullOrEmpty(ipAdressInformation.LocalhostHostName))
                    {
                        ipAdressInformation.ApdapterDnsSuffix = ipProps.DnsSuffix;
                        ipAdressInformation.LocalhostHostNameWithSuffix = $"{ipAdressInformation.LocalhostHostName}.{ipProps.DnsSuffix}";
                    }


                    ipAdressInformations.Add(ipAdressInformation);
                }

                networkInterfaceDetails.Add(new NetworkInterfaceDetail() { NetworkInterface = netInterface, IPInterfaceProperties = ipProps, IpAdressHostNames = ipAdressInformations }); ;
            }


            return networkInterfaceDetails;
        }
    }
}
