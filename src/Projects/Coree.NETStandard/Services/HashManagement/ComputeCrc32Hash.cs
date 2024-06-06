using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Hashing;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        /// <summary>
        /// Computes the CRC32 hash of data read from a stream asynchronously.
        /// </summary>
        /// <param name="stream">The stream from which to read the data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the read operation.</param>
        /// <returns>A task that represents the asynchronous operation, resulting in the CRC32 hash as a hexadecimal string.</returns>
        public async Task<uint> ComputeCrc32Async(Stream stream, CancellationToken cancellationToken = default)
        {
            try
            {
                if (stream == null)
                {
                    throw new ArgumentNullException(nameof(stream));
                }
                var crc32 = new Crc32(); // Make sure Crc32 implements NonCryptographicHashAlgorithm
                await crc32.AppendAsync(stream, cancellationToken);
                uint hashBytes = crc32.GetCurrentHashAsUInt32(); // This retrieves the computed hash

                return hashBytes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Computes the CRC32 hash of data read from a file asynchronously.
        /// </summary>
        /// <param name="filePath">The file path from which to read the data.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the read operation.</param>
        /// <returns>A task that represents the asynchronous operation, resulting in the CRC32 hash as a hexadecimal string.</returns>
        public async Task<uint> ComputeCrc32Async(string filePath, CancellationToken cancellationToken = default)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

    
            long fileSize = new FileInfo(filePath).Length;

            int bufferSize = ChooseBufferSize(fileSize);

            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
            {
                var result = await ComputeCrc32Async(stream, cancellationToken);

                return result;
            }
        }

        private int ChooseBufferSize(long fileSize)
        {
            const long size1MB = 1024 * 1024;
            const long size10MB = 10 * size1MB;
            const long size100MB = 100 * size1MB;
            const long size1GB = 1024 * size1MB;

            if (fileSize <= size1MB) return 4096;
            if (fileSize <= size10MB) return 8192;
            if (fileSize <= size100MB) return 16384;
            if (fileSize <= size1GB) return 65536;
            return 131072;
        }
    }
}