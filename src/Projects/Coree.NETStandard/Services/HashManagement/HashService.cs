using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Abstractions.ServiceFactoryEx;

using Microsoft.Extensions.Logging;

namespace Coree.NETStandard.Services.HashManagement
{
    /// <summary>
    /// Provides hashing services to manage and perform hash operations.
    /// </summary>
    public partial class HashService : ServiceFactoryEx<HashService>, IHashService
    {
        private readonly ILogger<HashService>? _logger;

        /// <summary>
        /// Initializes a new instance of the HashService with optional logging.
        /// </summary>
        /// <param name="logger">The logger used to log service activity and errors, if any.</param>
        public HashService(ILogger<HashService>? logger = null)
        {
            this._logger = logger;
        }
    }

    /// <summary>
    /// Defines the interface for a service that handles hashing operations.
    /// </summary>
    public interface IHashService
    {
        /// <summary>
        /// Computes the CRC32 hash of a given string using UTF-16 encoding, suitable for filenames.
        /// </summary>
        /// <param name="input">The string to hash, typically a filename.</param>
        /// <returns>A CRC32 hash as a hexadecimal string.</returns>
        string ComputeCrc32Hash(string input);
    }
}
