using System;

namespace Pi.Replicate.Worker.Host
{
	public class FolderWebhookChangedNotification
	{
		public Guid FolderId { get; set; }

		public string WebhookType { get; set; }

		public string CallbackUrl { get; set; }
	}
}
