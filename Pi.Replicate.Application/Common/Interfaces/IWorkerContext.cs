using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface IWorkerContext : IDisposable
    {
        IFolderRepository FolderRepository { get; }
        IFileRepository FileRepository { get; }
        IFileChunkRepository FileChunkRepository { get; }
        IChunkPackageRepository ChunkPackageRepository { get; }
        IRecipientRepository RecipientRepository { get; }
        IFailedFileRepository FailedFileRepository { get; }

    }
}
