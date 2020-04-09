using System;

namespace Pi.Replicate.Domain
{
    public class File
    {
        public Guid Id { get; set; }

		public Folder Folder { get; set; }

		public string Name { get; set; }

        public long Size { get; set; }

        public long AmountOfChunks { get; set; }

        public ReadOnlyMemory<byte> Hash { get; set; }

        public FileStatus Status { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Path { get; set; }

        public void UpdateForChange(System.IO.FileInfo file)
        {
            LastModifiedDate = file.LastWriteTimeUtc;
            Size = file.Length;
        }

        public static File BuildPartial(System.IO.FileInfo file, Folder folder, string basePath)
        {
            return new File
            {
                Id = Guid.NewGuid(),
                Folder = folder,
                LastModifiedDate = file.LastWriteTimeUtc,
                Name = file.Name,
                Path = file.FullName.Replace(basePath,""), //must be relative to base
                Size = file.Length,
                Status = FileStatus.New
            };
        }
	}
}