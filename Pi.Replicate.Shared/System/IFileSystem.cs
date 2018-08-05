using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.System
{
    public interface IFileSystem
    {
        bool DoesDirectoryExist(string path);
    }
}
