using System;
using System.Collections.Generic;
using System.IO.Hashing;
using System.Text;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.HashManagement
{
    public partial class HashService : ServiceFactoryEx<HashService>, IHashService
    {
        /// <summary>
        /// Computes the CRC32 hash of a given string using unicode encoding.
        /// </summary>
        /// <param name="input">The string to hash</param>
        /// <returns>A CRC32 hash as a hexadecimal string.</returns>
        public string ComputeCrc32Hash(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var crc32 = new Crc32();
            var bytes = Encoding.Unicode.GetBytes(input);
            crc32.Append(bytes);
            var hashBytes = crc32.GetCurrentHash();
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
