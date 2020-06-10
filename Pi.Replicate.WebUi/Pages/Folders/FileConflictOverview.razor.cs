using MediatR;
using Microsoft.AspNetCore.Components;
using Observr;
using Pi.Replicate.Application.FileConflicts.Commands.ResolveConflict;
using Pi.Replicate.Application.FileConflicts.Queries.GetFileConflicts;
using Pi.Replicate.WebUi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders
{
    public class FileConflictOverviewBase : ComponentBase
    {
		[Inject]
		public IMediator Mediator { get; set; }

		[Inject]
		public IBroker Broker { get; set; }

		[Parameter]
		public string FolderId { get; set; }

		public ICollection<FileConflictModel> Conflicts { get; set; } = new List<FileConflictModel>();

		protected override async Task OnInitializedAsync()
		{
			var conflictsResult = await Mediator.Send(new GetFileConflictsQuery { FolderId = Guid.Parse(FolderId) });
			if (conflictsResult.WasSuccessful)
			{
				Conflicts = conflictsResult.Data;
			}
		}

		protected async Task DeleteFile(FileConflictDeleteArgument arg)
		{
			await Mediator.Send(new ResolveConflictCommand { ToDeleteConflictId = arg.FileConflictId, ToDeleteFileId = arg.FileId });
			var foundConflict = Conflicts.Single(x => x.Id == arg.FileConflictId);
			Conflicts.Remove(foundConflict);
			if (!Conflicts.Any())
				await Broker.Publish(new FileConflictsResolvedMessage(Guid.Parse(FolderId)));
		}

	}
}
