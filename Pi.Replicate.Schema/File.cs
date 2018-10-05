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

        public string Hash { get; set; }

        public FileStatus Status { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Extension { get; set; }

        public Uri Source { get; set; }

        public string GetPath()
        {
            return System.IO.Path.Combine(Folder.GetPath(), Name); //TODO null check, also with Folder
        }
    }
}