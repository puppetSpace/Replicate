using Pi.Replicate.Application.Common;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderSettings
{
    public class FolderSettingsViewModel
    {
		public FolderSettingsViewModel()
		{
			Recipients = new List<Recipient>();
		}
		public ICollection<Recipient> Recipients { get; set; }

		public ICollection<FolderWebhookViewModel> FolderWebhooks { get; set; }
	}
}
