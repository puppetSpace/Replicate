using Microsoft.Data.SqlClient;
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
        Task<ICollection<FileChunk>> GetForFile(Guid fileId,int minSequenceNo=0,int maxSequenceNo=int.MaxValue);
        Task Create(FileChunk fileChunk);
        Task Create(FileChunk fileChunk, SqlConnection sqlConnection);
    }
}
