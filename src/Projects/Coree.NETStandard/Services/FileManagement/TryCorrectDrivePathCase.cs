using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.FileManagement
{
    public partial class FileService : ServiceFactoryEx<FileService>, IFileService
    {
        /// <summary>
        /// Attempts to retrieve the correctly cased drive root path based on a provided drive name, ignoring case sensitivity.
        /// </summary>
        /// <param name="drivename">The drive name to search for, case-insensitively.</param>
        /// <returns>The correctly cased drive root path if found; otherwise, returns the original drivename.</returns>
        /// <remarks>
        /// This method performs a case-insensitive comparison to find a matching drive among the available drives.
        /// If no matching drive is found or if an exception occurs during the drive search, the original drivename is returned.
        /// This ensures that the method fails gracefully, providing a fallback to the original input.
        /// </remarks>
        /// <example>
        /// <code>
        /// string drivePath = TryCorrectDrivePathCase("C:");
        /// Console.WriteLine(drivePath); // Output might be "C:\", or "C:" if no match is found
        /// </code>
        /// </example>
        public string? TryCorrectDrivePathCase(string? drivename)
        {
            if (string.IsNullOrWhiteSpace(drivename))
                return drivename;

            try
            {
                var driveInfos = DriveInfo.GetDrives();

                // Perform a case-insensitive comparison to find the first matching drive.
                string? matchedDrive = driveInfos.Select(e => e.Name).FirstOrDefault(e => e.Equals(drivename, StringComparison.OrdinalIgnoreCase));

                return matchedDrive ?? drivename;
            }
            catch
            {
                // In case of any exceptions, return the original drivename.
                return drivename;
            }
        }
    }
}