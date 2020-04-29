using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Commands.AddFolder;
using Pi.Replicate.Application.Folders.Queries.GetAvailableFolders;
using Pi.Replicate.Application.Recipients.Queries.GetVerifiedRecipients;
using Pi.Replicate.WebUi.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders
{
	public class AddFolderBase : ComponentBase
	{
		protected ICollection<string> AvailableFolderNames { get; set; } = new List<string>();
		protected string SelectedFolder { get; set; }
		protected FolderCreationOption SelectedFolderCreationOption { get; set; } = FolderCreationOption.CreateNew;
		protected List<CheckItem<Domain.Recipient>> Recipients { get; set; } = new List<CheckItem<Domain.Recipient>>();
		protected List<string> ValidationMessages { get; set; } = new List<string>();


		[Inject]
		protected IMediator Mediator { get; set; }

		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		protected override async Task OnInitializedAsync()
		{
			var folderNames = await Mediator.Send(new GetAvailableFoldersQuery());
			AvailableFolderNames = folderNames;
			var recipients = await Mediator.Send(new GetVerifiedRecipientsQuery());
			Recipients = recipients.OrderBy(x => x.Name).Select(x => new CheckItem<Domain.Recipient> { Data = x, DisplayText = x.Name }).ToList();
		}

		protected async Task CreateNewFolder()
		{
			var command = new AddFolderCommand
			{
				Name = SelectedFolder,
				Recipients = Recipients.Where(x => x.IsChecked).Select(x => x.Data).ToList(),
				CreateOnDisk = SelectedFolderCreationOption == FolderCreationOption.CreateNew
			};
			try
			{
				await Mediator.Send(command);
				ClearForm();
			}
			catch (ValidationException ex)
			{
				ValidationMessages = ex.Errors.Select(x => x.ErrorMessage).ToList();
			}
		}


		private void ClearForm()
		{
			var toRemovefolder = AvailableFolderNames.FirstOrDefault(x => string.Equals(x, SelectedFolder, StringComparison.OrdinalIgnoreCase));
			if (toRemovefolder is object)
				AvailableFolderNames.Remove(toRemovefolder);

			ValidationMessages.Clear();
			Recipients.ForEach(x => x.IsChecked = false);
			SelectedFolder = null;
			SelectedFolderCreationOption = FolderCreationOption.CreateNew;

		}
	}
	public enum FolderCreationOption
	{
		CreateNew = 0,
		SelectExisting = 1
	}
}
