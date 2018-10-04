using Pi.Replicate.Schema;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public interface IFolderRepository
    {
        Task<IEnumerable<Folder>> Get();
    }
}