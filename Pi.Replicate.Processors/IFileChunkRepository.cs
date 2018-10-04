using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public interface IFileChunkRepository
    {
        Task<IEnumerable<FileChunk>> Get(Guid fileId);

        Task Save(FileChunk fileChunk);
        void Delete(Guid fileId);
    }
}
