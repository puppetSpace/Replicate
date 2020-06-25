using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class FileTransmissionModel
	{
		public Guid Id { get; set; }

		public string FolderName { get; set; }

		public string Name { get; set; }

		public long Size { get; set; }

		public int Version { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string Path { get; set; }

		public string Host { get; set; }
	}
}
