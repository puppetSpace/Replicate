using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.System
{
    internal class WindowsFileSystem : IFileSystem
    {
        public bool DoesDirectoryExist(string path)
        {
            return !String.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }
    }
}
