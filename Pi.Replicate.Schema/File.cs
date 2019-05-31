using System;

namespace Pi.Replicate.Schema
{
    public class File
    {
        public Guid Id { get; set; }


		public Guid FolderId { get; set; }

		public Folder Folder { get; set; }

		public string Name { get; set; }

        public long Size { get; set; }

        public long AmountOfChunks { get; set; }

        public string Hash { get; set; }

        public FileStatus Status { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public string HostSource { get; set; }

        public string Path { get; set; }

		public string GetPath()
		{
			return Pi.Replicate.Shared.System.PathBuilder.Build(Folder.Name,Name);
		}
	}
}