using System;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Models
{
    public class FileDao
    {
        public Guid Id { get; set; }

        public Guid FolderId { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public int Version { get; set; }

        public FileSource Source { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Path { get; set; }
    }
}