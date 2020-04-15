using System;

namespace Pi.Replicate.Domain
{
    public class File
    {
        public Guid Id { get; private set; }

		public Folder Folder { get; private set; }

		public string Name { get; private set; }

        public long Size { get; private set; }

        public long AmountOfChunks { get; private set; }

        public byte[] Hash { get; private set; }

        public byte[] Signature { get; set; }

        public FileStatus Status { get; private set; }

        public DateTime LastModifiedDate { get; private set; }

        public string Path { get; private set; }

        public void MarkAsHandled()
        {
            Status = FileStatus.Handled;
        }

        public void UpdateForChange(System.IO.FileInfo file)
        {
            LastModifiedDate = file.LastWriteTimeUtc;
            Size = file.Length;
        }

        public void UpdateAfterProcessesing(int amountOfChunks, byte[] hash, byte[] signature)
        {
            Hash = hash;
            AmountOfChunks = amountOfChunks;
            Signature = signature;
            Status = FileStatus.Processed;
        }

        public static File BuildPartial(System.IO.FileInfo file, Folder folder, string basePath,DateTime? customLastModified = null)
        {
            if (file is null || !file.Exists)
                throw new InvalidOperationException($"Cannot created a File object for a file that does not exists: '{file?.FullName}'");

            return new File
            {
                Id = Guid.NewGuid(),
                Folder = folder,
                LastModifiedDate = customLastModified??file.LastWriteTimeUtc,
                Name = file.Name,
                Path = file.FullName.Replace(basePath+"\\",""), //must be relative to base
                Size = file.Length,
                Status = FileStatus.New
            };
        }
	}
}