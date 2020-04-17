using MediatR;
using Microsoft.AspNetCore.Components;
using Observr;
using Pi.Replicate.Application.Folders.Queries.GetFolderList;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Pi.Replicate.WebUi.Shared
{
    public class NavMenuBase : ComponentBase, Observr.IObserver<Folder>, IDisposable
    {
        private IDisposable _subscription;

        [Inject]
        public IBroker Broker { get; set; }

        [Inject]
        protected IMediator Mediator { get; set; }

        protected List<string> Folders { get; set; } = new List<string>();

        protected override async Task OnInitializedAsync()
        {
            _subscription?.Dispose();
            _subscription = Broker.Subscribe(this);
            var folderNames = await Mediator.Send(new GetFolderListQuery());
            Folders = folderNames.OrderBy(x=>x).ToList();
        }

        public async Task Handle(Folder value, CancellationToken cancellationToken)
        {
            await InvokeAsync(()=>
            {
                Folders.Add(value.Name);
                Folders = Folders.OrderBy(x=>x).ToList();
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            _subscription?.Dispose();
        }
    }
}
