using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces.Repositories
{
    public interface IFileRepository
    {
        Task<ICollection<File>> GetForFolder(Guid folderId);

        Task Create(File file);

        Task Update(File file);
    }
}
