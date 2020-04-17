using MediatR;
using Observr;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.AddFolder
{
	public class AddFolderCommand : IRequest
	{
		public string Name { get; set; }

		public bool DeleteAfterSend { get; set; }

		public bool CreateOnDisk { get; set; }

		public List<Recipient> Recipients { get; set; }
	}

	public class AddNewFolderCommandHandler : IRequestHandler<AddFolderCommand>
	{
		private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;
		private const string _insertStatementFolder = "INSERT INTO dbo.Folders(Id,Name,FolderOptions_DeleteAfterSent) VALUES(@Id,@Name,@DeleteAfterSent";
		private const string _insertStatementFolderRecipient = "INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,RecipientId)";
		private readonly IBroker _broker;

		public AddNewFolderCommandHandler(IDatabase database, PathBuilder pathBuilder, IBroker broker)
		{
			_database = database;
			_pathBuilder = pathBuilder;
			_broker = broker;
		}

		public async Task<Unit> Handle(AddFolderCommand request, CancellationToken cancellationToken)
		{
			var folder = new Folder
			{
				Id = Guid.NewGuid(),
				Name = request.Name,
				FolderOptions = new FolderOption { DeleteAfterSent = request.DeleteAfterSend },
			};

			folder.Recipients = request.Recipients;

			using (_database)
			{
				await _database.Execute(_insertStatementFolder, new { folder.Id, folder.Name, folder.FolderOptions.DeleteAfterSent });
				foreach (var recipient in folder.Recipients)
					await _database.Execute(_insertStatementFolderRecipient, new { FolderId = folder.Id, RecipientId = recipient.Id });
			}

			var path = _pathBuilder.BuildPath(folder.Name);
			if (request.CreateOnDisk && !System.IO.Directory.Exists(path))
				System.IO.Directory.CreateDirectory(path);


			await _broker.Publish(folder);
			return Unit.Value;
		}
	}
}
