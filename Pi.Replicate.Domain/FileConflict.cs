using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class FileConflict
    {
		public Guid Id { get; set; }

		public Guid FileId { get; set; }

		public FileConflictType Type { get; set; }

	}

	public enum FileConflictType
	{
		None = 0,
		[Description("Same version")]SameVersion = 1,
		[Description("Higher version")]HigherVersion = 2,
		[Description("Missing version")]MissingVersion = 3
	}
}
