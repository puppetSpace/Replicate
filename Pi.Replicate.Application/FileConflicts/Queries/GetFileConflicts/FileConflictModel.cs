using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileConflicts.Queries.GetFileConflicts
{
    public class FileConflictModel
    {
		public FileConflictModel()
		{
			ConflictingFiles = new List<FileModel>();
		}
		public Guid Id { get; set; }

		public FileConflictType Type { get; set; }

		public string RecipientName { get; set; }

		public FileModel File { get; set; }

		public ICollection<FileModel> ConflictingFiles { get; set; }
	}
}
