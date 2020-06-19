using MediatR;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Observr;
using Pi.Replicate.Application.Folders.Queries.GetFolderList;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.WebUi.Models;
using Pi.Replicate.WebUi.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Pi.Replicate.WebUi.Shared
{
	public class NavMenuBase : ComponentBase, Observr.IObserver<FileConflictsResolvedMessage>, Observr.IObserver<FolderAddedNotification>, IDisposable
	{
		private IDisposable _subscriptionFileConflictResolved;
		private IDisposable _sybscriptionFolderAdded;
		private HubConnection _hubConnection;

		[Inject]
		protected IBroker Broker { get; set; }

		[Inject]
		protected IMediator Mediator { get; set; }

		[Inject]
		protected HubProxy HubProxy { get; set; }

		protected List<FolderListItem> Folders { get; set; } = new List<FolderListItem>();

		protected override async Task OnInitializedAsync()
		{
			_subscriptionFileConflictResolved?.Dispose();
			_sybscriptionFolderAdded?.Dispose();
			_subscriptionFileConflictResolved = Broker.Subscribe<FileConflictsResolvedMessage>(this);
			_sybscriptionFolderAdded = Broker.Subscribe<FolderAddedNotification>(this);
			var folderResult = await Mediator.Send(new GetFolderListQuery());
			if (folderResult.WasSuccessful)
			{
				Folders = folderResult.Data.OrderBy(x => x.Name).ToList();

				_hubConnection = HubProxy.BuildConnection("communicationHub");
				_hubConnection.On<FolderAddedNotification>("FolderAdded", async x =>
				{
					await Handle(x, CancellationToken.None);
				});
			}
		}

		public Task Handle(FileConflictsResolvedMessage value, CancellationToken cancellationToken)
		{
			var foundFolder = Folders.Single(x => x.Id == value.FolderId);
			foundFolder.HasConflicts = false;
			StateHasChanged();
			return Task.CompletedTask;
		}

		public void Dispose()
		{
			_subscriptionFileConflictResolved?.Dispose();

		}

		public Task Handle(FolderAddedNotification value, CancellationToken cancellationToken)
		{
			return InvokeAsync(() =>
			{
				Log.Information("Notified of new folder");
				Folders.Add(new FolderListItem { Id = value.Id, Name = value.Name });
				Folders = Folders.OrderBy(x => x.Name).ToList();
				StateHasChanged();
			});
		}
	}
}
