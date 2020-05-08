using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Queries.GetFolderSettings;
using Pi.Replicate.Application.Recipients.Queries.GetVerifiedRecipients;
using Pi.Replicate.Domain;
using Pi.Replicate.WebUi.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders
{
	public class FolderSettingsBase : ComponentBase
	{
		[Inject]
		public IMediator Mediator { get; set; }

		[Parameter]
		public string FolderId { get; set; }

		public List<CheckItem<Recipient>> Recipients { get; set; } = new List<CheckItem<Recipient>>();

		//public FolderSettingsViewModel FolderSettingsViewModel { get; set; } = new FolderSettingsViewModel();

		protected override async Task OnInitializedAsync()
		{
			if (Guid.TryParse(FolderId, out var folderId))
			{
				var recipients = await Mediator.Send(new GetVerifiedRecipientsQuery());
				var folderSettingsResult = await Mediator.Send(new GetFolderSettingsQuery { FolderId = folderId });
				if (folderSettingsResult.WasSuccessful)
				{
					Recipients = recipients
						.OrderBy(x => x.Name)
						.Select(x => new CheckItem<Recipient> { Data = x, DisplayText = x.Name, IsChecked = folderSettingsResult.Data.Recipients.Any(y=>string.Equals(y.Name,x.Name,StringComparison.OrdinalIgnoreCase))})
						.ToList();
				}
			}
		}
	}
}
