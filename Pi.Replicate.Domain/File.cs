using System;
using System.Collections;
using System.Collections.Generic;

namespace Pi.Replicate.Domain
{
    public class File
    {
        public Guid Id { get; private set; }

		public Guid FolderId { get; private set; }

		public string Name { get; private set; }

		public int Version { get; private set; }

		public long Size { get; private set; }

        public ReadOnlyMemory<byte> Signature { get; private set; }

        public ReadOnlyMemory<byte> PreviousSignature { get; private set; }

		public FileSource Source { get; private set; }

        public FileStatus Status { get; private set; }

        public DateTime LastModifiedDate { get; private set; }

        public string Path { get; private set; }

        public void MarkAsHandled()
        {
            Status = FileStatus.Handled;
        }

        public void MarkAsProcessed()
        {
            Status = FileStatus.Processed;
        }

		public bool IsNew()
		{
			return Version == 1;
		}

        public void Update(System.IO.FileInfo file, ReadOnlyMemory<byte> signature)
        {
            LastModifiedDate = file.LastWriteTimeUtc;
            Size = file.Length;
			Version++;
            Status = FileStatus.Changed;
			PreviousSignature = Signature;
			Signature = signature;
        }

        public static File Build(System.IO.FileInfo file, Guid folderId, string basePath, ReadOnlyMemory<byte> signature, DateTime? customLastModified = null)
        {
            if (file is null || !file.Exists)
                throw new InvalidOperationException($"Cannot created a File object for a file that does not exists: '{file?.FullName}'");

			return new File
			{
				Id = Guid.NewGuid(),
				FolderId = folderId,
				LastModifiedDate = customLastModified ?? file.LastWriteTimeUtc,
				Name = file.Name,
				Path = file.FullName.Replace(basePath + "\\", ""), //must be relative to base
				Size = file.Length,
				Status = FileStatus.New,
				Source = FileSource.Local,
				Signature = signature,
				Version = 1
				
            };
        }

	
	}
}