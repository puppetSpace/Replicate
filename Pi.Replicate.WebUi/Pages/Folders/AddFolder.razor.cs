using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Commands.AddFolder;
using Pi.Replicate.Application.Folders.Queries.GetAvailableFolders;
using Pi.Replicate.Application.Recipients.Commands.AddRecipient;
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
        protected ICollection<string> AvailableFolderNames { get; set; } = new List<string>();
        protected string SelectedFolder { get; set; }
        protected FolderCreationOption SelectedFolderCreationOption { get; set; } = FolderCreationOption.CreateNew;
        protected bool DeleteAfterSend { get; set; }
        protected List<CheckItem<Domain.Recipient>> Recipients { get; set; } = new List<CheckItem<Domain.Recipient>>();
        protected List<string> ValidationMessages { get; set; } = new List<string>();
        protected List<string> RecipientValidationMessages { get; set; } = new List<string>();
        protected bool ShowCreateRecipientDialog { get; set; }
        protected RecipientModel RecipientModel { get; set; } = new RecipientModel();


        [Inject]
        protected IMediator Mediator { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var folderNames = await Mediator.Send(new GetAvailableFoldersQuery());
            AvailableFolderNames = folderNames;
            var recipients = await Mediator.Send(new GetRecipientListQuery());
            Recipients = recipients.OrderBy(x=>x.Name).Select(x => new CheckItem<Domain.Recipient> { Data = x, DisplayText = x.Name }).ToList();
        }

        protected async Task CreateNewFolder()
        {
            var command = new AddFolderCommand
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

        protected async Task CreateRecipient()
        {
            var command = new AddRecipientCommand
            {
                Name = RecipientModel.Name,
                Address = RecipientModel.Address,
            };
            try
            {
                var createdRecipient = await Mediator.Send(command);
                Recipients.Add(new CheckItem<Domain.Recipient> { Data = createdRecipient, DisplayText = createdRecipient.Name, IsChecked = true });
                RecipientModel = new RecipientModel();
                RecipientValidationMessages.Clear();
                ShowCreateRecipientDialog = false;
            }
            catch (ValidationException ex)
            {
                RecipientValidationMessages = ex.Errors.Select(x => x.ErrorMessage).ToList();
            }
        }

        private void ClearForm()
        {
            var toRemovefolder = AvailableFolderNames.FirstOrDefault(x => string.Equals(x, SelectedFolder, StringComparison.OrdinalIgnoreCase));
            if (toRemovefolder is object)
                AvailableFolderNames.Remove(toRemovefolder);

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

    public class RecipientModel
    {
        public string Name { get; set; }

        public string Address { get; set; }
    }
}
