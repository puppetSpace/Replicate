using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces.Repositories
{
    public interface IFolderRepository
    {
        Task<ICollection<Folder>> Get();

        Task Create(Folder folder);

        Task<bool> IsUnique(string name);
    }
}
