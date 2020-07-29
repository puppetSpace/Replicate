using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class FileConflict
	{
		public FileConflict()
		{

		}

		public FileConflict(Guid fileId, FileConflictType type)
		{
			FileId = fileId;
			Type = type;
		}

		public Guid FileId { get; private set; }

		public FileConflictType Type { get; private set; }
		
	}

	public enum FileConflictType
	{
		None = 0,
		SameVersion = 1,
		HigherVersion = 2,
		MissingVersion = 3
	}
}
