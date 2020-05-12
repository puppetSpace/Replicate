using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Files.Queries.GetFileOverviewForRecipient;
using Pi.Replicate.Application.Folders.Queries.GetFolderOverview;

namespace Pi.Replicate.WebUi.Pages.Folders
{
    public class FolderOverviewBase : ComponentBase
    {

        [Parameter]
        public string FolderId { get; set; }

        protected FolderOverviewModel FolderOverviewModel { get; set; } = new FolderOverviewModel();

		public List<FileOverviewModel> FileOverviewModels { get; set; } = new List<FileOverviewModel>();

		[Inject]
        protected IMediator Mediator { get; set; }


        protected override async Task OnInitializedAsync()
        {
            if (Guid.TryParse(FolderId, out var result))
            {
				var folderOverviewResult = await Mediator.Send(new GetFolderOverviewQuery { FolderId = result });
				if (folderOverviewResult.WasSuccessful)
					FolderOverviewModel = folderOverviewResult.Data;


				var queryResult = await Mediator.Send(new GetFileOverviewForRecipientQuery { FolderId = Guid.Parse(FolderId), RecipientId = FolderOverviewModel.Recipients.First().RecipientId });
				if (queryResult.WasSuccessful)
					FileOverviewModels = queryResult.Data.ToList();
			}
        }

        protected async Task RecipientClicked(Guid recipientId)
		{
			var queryResult = await Mediator.Send(new GetFileOverviewForRecipientQuery { FolderId = Guid.Parse(FolderId), RecipientId = recipientId });
			if (queryResult.WasSuccessful)
				FileOverviewModels = queryResult.Data.ToList();
		}
    }
}