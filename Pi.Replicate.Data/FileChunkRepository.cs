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
            var foundFileChunks = await GetForFile(fileId);

            if (foundFileChunks.Any())
            {
                _replicateDbContext.FileChunks.RemoveRange(foundFileChunks);
                await _replicateDbContext.SaveChangesAsync();
            }

        }

		public async Task<FileChunk> Get(Guid fileChunkId)
		{
			return await _replicateDbContext.FileChunks
				.FirstOrDefaultAsync(x => x.Id == fileChunkId);
		}


		public async Task<IEnumerable<FileChunk>> GetForFile(Guid fileId)
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

        public async Task SaveFailed(HostFileChunk failedUploadFileChunk)
        {
            _replicateDbContext.FailedUploadFileChunks.Add(failedUploadFileChunk);
            await _replicateDbContext.SaveChangesAsync();
        }
    }
}
