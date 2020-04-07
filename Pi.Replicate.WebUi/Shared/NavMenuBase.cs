using MediatR;
using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Commands.AddNewFolder;
using Pi.Replicate.Application.Folders.Queries.GetFolderList;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Pi.Replicate.WebUi.Shared
{
    public class NavMenuBase : ComponentBase, IAddNewFolderObserver
    {
        private IDisposable _subscription;

        [Inject]
        public AddNewFolderSubject  AddNewFolderSubject { get; set; }

        [Inject]
        protected IMediator Mediator { get; set; }

        protected List<FolderLookupDto> Folders { get; set; } = new List<FolderLookupDto>();

        public NavMenuBase()
        {

        }

        protected override async Task OnInitializedAsync()
        {
            _subscription?.Dispose();
            _subscription = AddNewFolderSubject.Subscribe(this);
            var vm = await Mediator.Send(new GetFolderListQuery());
            Folders = vm.Folders.OrderBy(x=>x.Name).ToList();
        }

        public void Notify(Folder newFolder)
        {
            Folders.Add(new FolderLookupDto { Name = newFolder.Name });
            Folders = Folders.OrderBy(x => x.Name).ToList();
            InvokeAsync(StateHasChanged);
        }
    }
}
