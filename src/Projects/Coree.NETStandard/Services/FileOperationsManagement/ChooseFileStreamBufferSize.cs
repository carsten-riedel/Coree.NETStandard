using Coree.NETStandard.Abstractions.ServiceFactoryEx;

namespace Coree.NETStandard.Services.FileOperationsManagement
{
    public partial class FileOperationsService : ServiceFactoryEx<FileOperationsService>, IFileOperationsService
    {
        private int ChooseFileStreamBufferSize(long fileSize)
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
