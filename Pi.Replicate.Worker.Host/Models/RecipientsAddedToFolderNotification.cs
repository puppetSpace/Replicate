using System;
using System.Collections.Generic;

namespace Pi.Replicate.Worker.Host
{
	public class RecipientsAddedToFolderNotification
	{
		public Guid FolderId { get; set; }

		public List<Guid> Recipients { get; set; }
	}
}
