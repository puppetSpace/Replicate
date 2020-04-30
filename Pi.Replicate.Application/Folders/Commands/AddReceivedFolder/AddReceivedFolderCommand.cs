using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.AddReceivedFolder
{
    public class AddReceivedFolderCommand : IRequest<Guid>
    {
		public string Name { get; set; }

		public string Sender { get; set; }
	}

	public class AddReceivedFolderCommandHandler : IRequestHandler<AddReceivedFolderCommand, Guid>
	{
		private const string _folderInsertStatement = @"INSERT INTO dbo.Folder(Id,Name) VALUES(@Id,@Name)";
		private const string _folderIdSelectStatement = "SELECT Id FROM dbo.Folder WHERE Name = @Name";
		private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;

		public AddReceivedFolderCommandHandler(IDatabase database, PathBuilder pathBuilder)
		{
			_database = database;
			_pathBuilder = pathBuilder;
		}

		public async Task<Guid> Handle(AddReceivedFolderCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var folderId = await _database.QuerySingle<Guid>(_folderIdSelectStatement, new { request.Name });
				if (folderId == Guid.Empty)
				{
					folderId = Guid.NewGuid();
					await _database.Execute(_folderInsertStatement, new { Id = folderId, request.Name });
					var folderPath = _pathBuilder.BuildPath(request.Name);
					if (!System.IO.Directory.Exists(folderPath))
						System.IO.Directory.CreateDirectory(folderPath);

					//todo link sender
				}

				return folderId;
			}
		}
	}
}
