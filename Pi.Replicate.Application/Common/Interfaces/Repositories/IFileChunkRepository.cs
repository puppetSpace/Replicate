using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces.Repositories
{
    public interface IFileChunkRepository
    {
        Task<ICollection<FileChunk>> GetForFile(Guid fileId);
        Task Create(FileChunk fileChunk);
    }
}
