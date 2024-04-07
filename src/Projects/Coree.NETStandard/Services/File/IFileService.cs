using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Coree.NETStandard.Services.File
{
    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    public interface IFileService
    {
        string? GetCorrectCasedPath(string? path);
        Task<string?> GetCorrectCasedPathAsync(string? path, CancellationToken cancellationToken);
        string? IsCommandAvailable(string? command);
        Task<string?> IsCommandAvailableAsync(string? command, CancellationToken cancellationToken);
    }
}
