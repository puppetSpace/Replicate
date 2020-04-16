using Microsoft.Data.SqlClient;
using Pi.Replicate.Application.Common.Interfaces.Repositories;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface IWorkerContext
    {
        IFolderRepository FolderRepository { get; }
        IFileRepository FileRepository { get; }
        IFileChunkRepository FileChunkRepository { get; }
        IChunkPackageRepository ChunkPackageRepository { get; }
        IRecipientRepository RecipientRepository { get; }
        IFailedFileRepository FailedFileRepository { get; }

        SqlConnection BuildConnection();

    }
}
