using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Coree.NETStandard.Abstractions.ServiceFactory;
using Coree.NETStandard.Services.FileService;
using Microsoft.Extensions.Logging;
using static Coree.NETStandard.Services.FileOperationsService.FileOperationsService;

namespace Coree.NETStandard.Services.FileOperationsService
{
    public partial class FileOperationsService : ServiceFactory<FileOperationsService>, IFileOperationsService
    {
        private readonly ILogger<FileOperationsService>? _logger;

        public FileOperationsService(ILogger<FileOperationsService>? logger = null)
        {
            this._logger = logger;
        }
    }

    /// <summary>
    /// Defines a service for file system operations.
    /// </summary>
    public interface IFileOperationsService
    {
        bool FileCopy(string source,string destination);

        Task<bool> FileCopyAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, CancellationToken cancellationToken = default);

        Task<bool> FileCopierAsync(string source, string destination, int maxRetryCount = 3, int retryDelay = 1000, int rewindBlocks = 3, CancellationToken cancellationToken = default);

        bool DeleteFile(string location);

        Task<VerifiedCopyStatus> RetryVerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default, int maxRetryCount = 3, int retryDelayMilliseconds = 1000);

        Task<VerifiedCopyStatus> VerifyAndResumeFileCopyAsync(string source, string destination, CancellationToken cancellationToken = default);

        Task CreateJsonPathInventoryAsync(string? path, string? inventoryFilename = "");

        Task InventoryCopyAsync(string inventoryFilename, string target);

    }
}

