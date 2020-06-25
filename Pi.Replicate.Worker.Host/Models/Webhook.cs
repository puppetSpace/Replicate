using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class Webhook
	{
		public Guid FolderId { get; set; }

		public string Type { get; set; }

		public string CallbackUrl { get; set; }
	}
}
