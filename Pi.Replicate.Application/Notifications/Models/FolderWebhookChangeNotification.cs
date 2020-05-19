using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Notifications.Models
{
    public class FolderWebhookChangeNotification
    {
		public Guid FolderId { get; set; }

		public string WebhookType { get; set; }

		public string CallbackUrl { get; set; }
	}
}
