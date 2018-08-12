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
        IEnumerable<File> Get(Guid folderId);

        void Save(File file);

    }
}
