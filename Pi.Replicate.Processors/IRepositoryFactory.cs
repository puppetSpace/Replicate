using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public interface IRepositoryFactory
    {
        IFileRepository CreateFileRepository();
        IFolderRepository CreateFolderRepository();
    }
}
