using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.DeleteFolder
{
    public class DeleteFolderCommand : IRequest<Result>
    {
		public Guid FolderId { get; set; }
	}

	public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _deleteStatement = "DELETE FROM dbo.Folder WHERE Id = @FolderId";

		public DeleteFolderCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_deleteStatement, new { request.FolderId });

			return Result.Success();
		}
	}
}
