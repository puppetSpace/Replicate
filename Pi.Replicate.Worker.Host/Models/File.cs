using System;
using System.Collections;
using System.Collections.Generic;

namespace Pi.Replicate.Worker.Host.Models
{
    public class File
    {
        public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public string Name { get; set; }

		public int Version { get; set; }

		public long Size { get; set; }

		public FileSource Source { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string Path { get; set; }

		public bool IsNew() => Version == 1;

        public void Update(System.IO.FileInfo file)
        {
			Id = Guid.NewGuid();
            LastModifiedDate = file.LastWriteTimeUtc;
            Size = file.Length;
			Version++;

        }

        public static File Build(System.IO.FileInfo file, Guid folderId, string basePath, DateTime? customLastModified = null)
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
				Source = FileSource.Local,
				Version = 1
				
            };
        }
	}

	public class RequestFile : File
	{
		public ICollection<Recipient> Recipients { get; set; }
	}

	public enum FileSource
	{
		Local = 0,
		Remote = 1
	}
}