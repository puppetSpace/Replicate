using System;

namespace Pi.Replicate.Schema
{
    public class File
    {
        public Guid Id { get; set; }

        public Folder Folder { get; set; }

        public string Name { get; set; }

        public long Size { get; set; }

        public long AmountOfChunks { get; set; }

        public FileStatus Status { get; set; }
    }
}