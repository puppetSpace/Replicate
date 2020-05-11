using Pi.Replicate.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderSettings
{
	public class FolderWebhookViewModel : ViewModelBase
	{
		private string _callbackUrl;

		public string CallbackUrl { get => _callbackUrl; set => Set(ref _callbackUrl,value); }

		public Guid WebhookTypeId { get; set; }

		public string WebHookTypeName { get; set; }

		public string WebHookTypeDescription { get; set; }

		public string WebHookTypeMessageStructure { get; set; }

	}
}
