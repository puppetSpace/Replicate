using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class FolderWebhook
    {
		public Guid Id { get; set; }

		public Guid FolderId { get; set; }

		public WebhookType WebhookType { get; set; }

		public string CallbackUrl { get; set; }
	}
}
