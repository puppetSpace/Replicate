using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileConflicts.Queries.GetFileConflicts
{
    public class FileModel
    {
		public Guid Id { get; private set; }

		public string Name { get; private set; }

		public int Version { get; private set; }

		public DateTime LastModifiedDate { get; private set; }

		public string Path { get; private set; }

		public string RecipientName { get; set; }

	}
}
