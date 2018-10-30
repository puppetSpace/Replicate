using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Repositories
{
    public interface IFileRepository
    {
        Task<IEnumerable<File>> GetSent(Guid folderId);

        Task<IEnumerable<File>> GetReceived();

        Task Save(File file);
        Task Update(File file);
        Task SaveTemp(TempFile file);
        Task<TempFile> GetTempFileIfExists(File file);
        Task DeleteTempFile(Guid fileId);
        Task<IEnumerable<File>> GetCompletedReceivedFiles();
        Task Delete(Guid id);
        Task<File> Get(Guid fileId);
    }
}
