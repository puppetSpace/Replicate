using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Files
{
    internal static class FileBuilder
    {
        internal static File Build(Folder folder, System.IO.FileInfo fileInfo)
        {
            return new Schema.File
            {
                Id = Guid.NewGuid(),
                Folder = folder,
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                Status = FileStatus.New
            };
        }
    }
}
