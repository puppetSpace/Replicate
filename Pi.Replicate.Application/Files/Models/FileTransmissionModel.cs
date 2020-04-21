using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Models
{
    public class FileTransmissionModel
    {
        public string FolderName { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public long AmountOfChunks { get; set; }

        public ReadOnlyMemory<byte> Signature { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Path { get; set; }
    }
}
