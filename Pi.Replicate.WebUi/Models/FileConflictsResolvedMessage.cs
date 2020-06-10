using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Models
{
    public class FileConflictsResolvedMessage
    {
		public FileConflictsResolvedMessage(Guid folderId)
		{
			FolderId = folderId;
		}

		public Guid FolderId { get;}
	}
}
