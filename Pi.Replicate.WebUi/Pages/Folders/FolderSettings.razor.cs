using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Pi.Replicate.Application.Folders.Commands.AddRecipientToFolder;
using Pi.Replicate.Application.Folders.Commands.DeleteFolder;
using Pi.Replicate.Application.Folders.Queries.GetFolderSettings;
using Pi.Replicate.Application.FolderWebhooks.Commands.UpsertFolderWebhook;
using Pi.Replicate.Application.Recipients.Queries.GetVerifiedRecipients;
using Pi.Replicate.Domain;
using Pi.Replicate.WebUi.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders
{
	public class FolderSettingsBase : ComponentBase
	{
		private List<Recipient> _originalSelectedRecipients;

		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Parameter]
		public string FolderId { get; set; }

		public List<CheckItem<Recipient>> Recipients { get; set; } = new List<CheckItem<Recipient>>();

		public List<FolderWebhookViewModel> FolderWebhookViewModels { get; set; } = new List<FolderWebhookViewModel>();

		public List<string> ValidationMessages { get; set; } = new List<string>();

		protected override async Task OnInitializedAsync()
		{
			if (Guid.TryParse(FolderId, out var folderId))
			{
				var recipientsResult = await Mediator.Send(new GetVerifiedRecipientsQuery());
				var folderSettingsResult = await Mediator.Send(new GetFolderSettingsQuery { FolderId = folderId });
				if (folderSettingsResult.WasSuccessful && recipientsResult.WasSuccessful)
				{
					Recipients = recipientsResult.Data
						.OrderBy(x => x.Name)
						.Select(x => new CheckItem<Recipient> { Data = x, DisplayText = x.Name, IsChecked = folderSettingsResult.Data.Recipients.Any(y => string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase)) })
						.ToList();

					FolderWebhookViewModels = folderSettingsResult.Data.FolderWebhooks.ToList();
					_originalSelectedRecipients = folderSettingsResult.Data.Recipients.ToList();
				}
			}
		}

		protected async Task Save()
		{
			ValidationMessages.Clear();
			if (!Recipients.Any(x => x.IsChecked))
			{
				ValidationMessages.Add("Atleast one recipient should be selected.");
				return;
			}
			var selectedRecipients = Recipients.Where(x => x.IsChecked).Select(x => x.Data).ToList();
			var deletedRecipients = _originalSelectedRecipients.Where(x => !selectedRecipients.Any(y => y.Id == x.Id)).Select(x => x.Id);
			var addedRecipients = selectedRecipients.Where(x => !_originalSelectedRecipients.Any(y => y.Id == x.Id)).Select(x => x.Id);

			var parsedFolderId = Guid.Parse(FolderId);

			await Mediator.Send(new UpdateRecipientsForFolderCommand { FolderId = parsedFolderId, ToAddRecipients = addedRecipients.ToList(), ToDeleteRecipients = deletedRecipients.ToList() });

			foreach (var fwh in FolderWebhookViewModels.Where(x => x.IsChanged))
				await Mediator.Send(new UpsertFolderWebhookCommand { FolderId = parsedFolderId, WebhookTypeId = fwh.WebhookTypeId, WebhookTypeName = fwh.WebHookTypeName, CallbackUrl = fwh.CallbackUrl });

			foreach (var newRecipient in addedRecipients)
			{
				//todo notify. Start processing existing files for these recipients. should be done in de command
			}

			NavigationManager.NavigateTo($"/folder/{FolderId}");
		}

		protected async Task Delete()
		{
			var mayDelete = await JSRuntime.InvokeAsync<bool>("deleteFolderRequest");
			if (mayDelete)
			{
				await Mediator.Send(new DeleteFolderCommand { FolderId = Guid.Parse(FolderId) });
				NavigationManager.NavigateTo("/");
			}
		}
	}
}
