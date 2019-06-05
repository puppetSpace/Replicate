using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Repositories
{
    public interface IFolderRepository
    {
        Task<IEnumerable<Folder>> Get();

		Task<IEnumerable<FolderOption>> GetFolderOptions(Guid folderId);

        Task Save(Folder folder);

		Task SaveFolderOptions(IEnumerable<FolderOption> folderOptions);
    }
}