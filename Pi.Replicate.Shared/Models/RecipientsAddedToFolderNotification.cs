using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.Models
{
	public class RecipientsAddedToFolderNotification
	{
		public Guid FolderId { get; set; }

		public List<Guid> Recipients { get; set; }
	}
}
