using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Repositories
{
    public interface IFileChunkRepository
    {
		Task<FileChunk> Get(Guid fileChunkId);
		Task<IEnumerable<FileChunk>> GetForFile(Guid fileId);
        Task Save(FileChunk fileChunk);
        Task DeleteForFile(Guid fileId);
        Task SaveFailed(HostFileChunk failedUploadFileChunk);
    }
}
