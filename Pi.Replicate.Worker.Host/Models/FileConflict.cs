using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Models
{
    public class FileConflict
    {
		public Guid FileId { get; private set; }

		public FileConflictType Type { get; private set; }

		public static FileConflict Create(Guid fileId, FileConflictType type)
		{
			return new FileConflict
			{
				FileId = fileId,
				Type = type
			};
		}
	}

	public enum FileConflictType
	{
		None = 0,
		SameVersion = 1,
		HigherVersion = 2,
		MissingVersion = 3
	}
}
