using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Components;

namespace Pi.Replicate.WebUi.Pages.Folders
{
    public class FolderOverviewBase : ComponentBase
    {

        [Parameter]
        public string FolderId { get; set; }

        [Inject]
        protected IMediator Mediator { get; set; }


        protected override Task OnInitializedAsync()
        {

        }
    }
}