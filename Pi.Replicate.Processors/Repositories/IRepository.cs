using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Repositories
{
    public interface IRepository
    {
        IFileChunkRepository FileChunkRepository { get; }

        IFileRepository FileRepository { get; }

        IFolderRepository FolderRepository { get; }

        IHostRepository HostRepository { get; }
    }
}
