using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Queries.GetFolderOverview;

namespace Pi.Replicate.WebUi.Pages.Folders
{
    public class FolderOverviewBase : ComponentBase
    {

        [Parameter]
        public string FolderId { get; set; }

        protected FolderOverviewModel FolderOverviewModel { get; set; } = new FolderOverviewModel();

        [Inject]
        protected IMediator Mediator { get; set; }


        protected override async Task OnInitializedAsync()
        {
            if (Guid.TryParse(FolderId, out var result))
            {
				FolderOverviewModel = await Mediator.Send(new GetFolderOverviewQuery { FolderId = result });
			}
        }

        protected void RecipientClicked(Guid recipientId)
        {
            Debug.WriteLine(recipientId);
        }
    }
}