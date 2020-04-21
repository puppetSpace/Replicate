using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.UpdateChangedFiles
{
    public class FoundToUpdateFileDto
    {
        public Guid Id { get; set; }

        public Guid FolderId { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public long AmountOfChunks { get; set; }

        public byte[] Signature { get; set; }

        public FileSource Source { get; set; }

        public FileStatus Status { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Path { get; set; }
    }
}
