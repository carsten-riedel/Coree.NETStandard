using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.NetworkManagement
{
    public partial class NetworkService : ServiceFactoryEx<NetworkService>, INetworkService
    {
        public void doo()
        {
            // Get host name
            String strHostName = Dns.GetHostName();
            var ss = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();
            
            

            // Find host by name
            IPHostEntry iphostentry = Dns.GetHostByName(strHostName);

            // Enumerate IP addresses
            foreach (IPAddress ipaddress in iphostentry.AddressList)
            {
                
            }

            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine("Name: " + netInterface.Name);
                Console.WriteLine("Description: " + netInterface.Description);
                Console.WriteLine("Addresses: ");

                IPInterfaceProperties ipProps = netInterface.GetIPProperties();

                foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    Console.WriteLine(" " + addr.Address.ToString());
                }

                Console.WriteLine("");
            }
        }

        public IPAddress[] GetIPAddresses(string domainName)
        {
            try
            {
                IPAddress[] addresses = Dns.GetHostAddresses(domainName);
                return addresses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resolving DNS: {ex.Message}");
                return new IPAddress[0]; // Return an empty array on failure
            }
        }

    }
}
