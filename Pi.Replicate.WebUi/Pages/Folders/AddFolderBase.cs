using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Commands.AddNewFolder;
using Pi.Replicate.Application.Folders.Queries.GetAvailableFolders;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientList;
using Pi.Replicate.WebUi.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders
{
    public class AddFolderBase : ComponentBase
    {
        protected List<AvailableFolderDto> AvailableFolders { get; set; } = new List<AvailableFolderDto>();
        protected string SelectedFolder { get; set; }
        protected FolderCreationOption SelectedFolderCreationOption { get; set; } = FolderCreationOption.CreateNew;
        protected bool DeleteAfterSend { get; set; }
        protected List<CheckItem<Domain.Recipient>> Recipients { get; set; } = new List<CheckItem<Domain.Recipient>>();
        protected List<string> ValidationMessages { get; set; } = new List<string>();

        [Inject]
        protected IMediator Mediator { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var foldersVm = await Mediator.Send(new GetAvailableFoldersQuery());
            AvailableFolders = foldersVm.Folders;
            var recipientsVm = await Mediator.Send(new GetRecipientListQuery());
            Recipients = recipientsVm.Recipients.OrderBy(x=>x.Name).Select(x => new CheckItem<Domain.Recipient> { Data = x, DisplayText = x.Name }).ToList();
        }

        protected async Task CreateNewFolder()
        {
            var command = new AddNewFolderCommand
            {
                Name = SelectedFolder,
                DeleteAfterSend = DeleteAfterSend,
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
            var toRemovefolder = AvailableFolders.FirstOrDefault(x => string.Equals(x.Name, SelectedFolder, StringComparison.OrdinalIgnoreCase));
            if (toRemovefolder is object)
                AvailableFolders.Remove(toRemovefolder);

            ValidationMessages.Clear();
            Recipients.ForEach(x => x.IsChecked = false);
            DeleteAfterSend = false;
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
