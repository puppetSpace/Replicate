using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.Models
{
	public class FolderWebhookChangedNotification
	{
		public Guid FolderId { get; set; }

		public string WebhookType { get; set; }

		public string CallbackUrl { get; set; }
	}
}
