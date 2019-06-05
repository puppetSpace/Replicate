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
    internal sealed class FolderRepository : IFolderRepository
    {
        private ReplicateDbContext _replicateDbContext;

        public FolderRepository(ReplicateDbContext replicateDbContext)
        {
            _replicateDbContext = replicateDbContext;
        }

        public async Task<IEnumerable<Folder>> Get()
        {
            return await _replicateDbContext.Folders
                .ToListAsync();
        }

		public async Task<IEnumerable<FolderOption>> GetFolderOptions(Guid folderId)
		{
			return await _replicateDbContext.FolderOptions
				.Where(x => x.FolderId == folderId)
				.ToListAsync();
		}

		public async Task Save(Folder folder)
        {
            _replicateDbContext.Folders.Add(folder);
            await _replicateDbContext.SaveChangesAsync();
        }

		public async Task SaveFolderOptions(IEnumerable<FolderOption> folderOptions)
		{
			_replicateDbContext.FolderOptions.AddRange(folderOptions);
			await _replicateDbContext.SaveChangesAsync();
		}
	}
}
