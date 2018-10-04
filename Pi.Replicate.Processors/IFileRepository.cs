using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public interface IFileRepository
    {
        Task<IEnumerable<File>> GetSent(Guid folderId);

        Task<IEnumerable<File>> GetReceived();

        Task Save(File file);
        void Update(File file);
    }
}
