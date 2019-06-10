using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Data
{
    internal sealed class FileRepository : IFileRepository
    {
        private ReplicateDbContext _replicateDbContext;
        //todo check includes. Could be that soms foreign object need to be included to work correctly. Lazy loading is disabled

        public FileRepository(ReplicateDbContext replicateDbContext)
        {
            _replicateDbContext = replicateDbContext;
        }

        public async Task Delete(Guid id)
        {
            var foundFile = await _replicateDbContext.Files.FindAsync(id);
            if (foundFile != null)
            {
                _replicateDbContext.Files.Remove(foundFile);
                await _replicateDbContext.SaveChangesAsync();
            }
        }

		public async Task DeleteNewFiles()
		{
			var toDeleteFiles = _replicateDbContext.Files
				.Where(x => x.Status == FileStatus.New);
			foreach (var file in toDeleteFiles)
				_replicateDbContext.Remove(file);

			await _replicateDbContext.SaveChangesAsync();
		}

		public async Task DeleteTempFile(Guid fileId)
        {
            var foundTempFile = await _replicateDbContext.TempFiles.FindAsync(fileId);
            if (foundTempFile != null)
            {
                _replicateDbContext.TempFiles.Remove(foundTempFile);
                await _replicateDbContext.SaveChangesAsync();
            }
        }

        public async Task<File> Get(Guid fileId)
        {
            return await _replicateDbContext.Files.FindAsync(fileId);
        }

        public Task<IEnumerable<File>> GetCompletedReceivedFiles()
        {
            //todo lock and Set flag that these files are being processed
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<File>> GetReceived()
        {
            return await _replicateDbContext.Files
                .Where(x => x.Status == FileStatus.ReceivedComplete)
                .ToListAsync();
        }

        public async Task<IEnumerable<File>> GetSent(Guid folderId)
        {
            return await _replicateDbContext.Files
                .Where(x => x.FolderId == folderId 
				&& (x.Status == FileStatus.New 
					|| x.Status == FileStatus.Sent 
					|| x.Status == FileStatus.UploadSucessful))
                .ToListAsync();
        }

        public async Task<TempFile> GetTempFileIfExists(File file)
        {
            return await _replicateDbContext.TempFiles.FirstOrDefaultAsync(x => x.FileId == file.Id);
        }

        public async Task Save(File file)
        {
            _replicateDbContext.Files.Add(file);
            await _replicateDbContext.SaveChangesAsync();
        }

        public async Task SaveTemp(TempFile file)
        {
            _replicateDbContext.TempFiles.Add(file);
            await _replicateDbContext.SaveChangesAsync();
        }

        public async Task Update(File file)
        {
            _replicateDbContext.Files.Update(file);
            await _replicateDbContext.SaveChangesAsync();
        }
    }
}
