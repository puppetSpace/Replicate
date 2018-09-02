using Pi.Replicate.Schema;
using System.Collections.Generic;

namespace Pi.Replicate.Processors
{
    public interface IFolderRepository
    {
        IEnumerable<Folder> Get();
    }
}