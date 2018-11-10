using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data
{
    internal sealed class FileChunkRepository : IFileChunkRepository
    {
        private ReplicateDbContext _replicateDbContext;

        public FileChunkRepository(ReplicateDbContext replicateDbContext)
        {
            _replicateDbContext = replicateDbContext;
        }

        public async Task DeleteForFile(Guid fileId)
        {
            var foundFileChunks = await Get(fileId);

            if (foundFileChunks.Any())
            {
                _replicateDbContext.FileChunks.RemoveRange(foundFileChunks);
                await _replicateDbContext.SaveChangesAsync();
            }

        }

        public async Task<IEnumerable<FileChunk>> Get(Guid fileId)
        {
            return await _replicateDbContext.FileChunks
               .Include(x => x.File)
               .Where(x => x.File.Id == fileId)
               .ToListAsync();
        }

        public async Task Save(FileChunk fileChunk)
        {
            _replicateDbContext.FileChunks.Add(fileChunk);
            await _replicateDbContext.SaveChangesAsync();
        }

        public async Task SaveFailed(FailedUploadFileChunk failedUploadFileChunk)
        {
            _replicateDbContext.FailedUploadFileChunks.Add(failedUploadFileChunk);
            await _replicateDbContext.SaveChangesAsync();
        }
    }
}
