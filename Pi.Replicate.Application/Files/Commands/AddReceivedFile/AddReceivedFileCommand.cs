using MediatR;
using Observr;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddReceivedFile
{
	public class AddReceivedFileCommand : IRequest<Result>
	{
		public Guid Id { get; set; }

		public string FolderName { get; set; }

		public string Name { get; set; }

		public long Size { get; set; }

		public int Version { get; set; }

		public byte[] Signature { get; set; }

		public DateTime LastModifiedDate { get; set; }

		public string Path { get; set; }

		public string Sender { get; set; }

		public string SenderAddress { get; set; }
	}

	public class AddReceivedFileCommandHandler : IRequestHandler<AddReceivedFileCommand, Result>
	{
		private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;
		private readonly IBroker _broker;
		private const string _folderInsertStatement = @"INSERT INTO dbo.Folder(Id,Name) VALUES(@Id,@Name)";
		private const string _folderIdSelectStatement = "SELECT Id FROM dbo.Folder WHERE Name = @Name";
		private const string _fileInsertStatement = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.[File] WHERE Id = @Id)
				INSERT INTO dbo.[File](Id,FolderId, Name, Size,Version,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @Version, @LastModifiedDate,@Path, @Signature, @Source)
			ELSE
				UPDATE dbo.[File] SET Size = @Size, LastModifiedDate = @LastModifiedDate, Signature = @Signature WHERE Id = @Id";

		private const string _recipientCreationStatement = @"
			BEGIN
				DECLARE @recipientId uniqueidentifier;

				SELECT @recipientId = Id
				FROM dbo.Recipient
				WHERE [Name] = @Name;

				IF(@recipientId is null)
				BEGIN
					SET @recipientId = NEWID();
					INSERT INTO dbo.Recipient(Id,[Name],[Address], Verified) VALUES(@recipientId,@Name,@Address,1);
				END

				IF NOT EXISTS (SELECT 1 FROM dbo.FolderRecipient WHERE FolderId = @FolderId and RecipientId = @recipientId)
					INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,@recipientId);
			END";

		public AddReceivedFileCommandHandler(IDatabase database, PathBuilder pathBuilder, IBroker broker)
		{
			_database = database;
			_pathBuilder = pathBuilder;
			_broker = broker;
		}

		public async Task<Result> Handle(AddReceivedFileCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var folderId = await CreateFolderIfNotExists(request.FolderName);
				await _database.Execute(_fileInsertStatement, new { request.Id, FolderId = folderId, request.Name, request.Size, request.Version, request.LastModifiedDate, request.Path, request.Signature, Source = FileSource.Remote });
				await _database.Execute<Recipient>(_recipientCreationStatement, new { Address = request.SenderAddress, Name = request.Sender, FolderId = folderId });
			}

			return Result.Success();
		}

		private async Task<Guid> CreateFolderIfNotExists(string folderName)
		{
			var folderId = await _database.QuerySingle<Guid>(_folderIdSelectStatement, new { Name = folderName });
			if (folderId == Guid.Empty)
			{
				Log.Verbose($"Creating new folder {folderName}");
				folderId = Guid.NewGuid();
				await _database.Execute(_folderInsertStatement, new { Id = folderId, Name = folderName });
				var folderPath = _pathBuilder.BuildPath(folderName);
				if (!System.IO.Directory.Exists(folderPath))
					System.IO.Directory.CreateDirectory(folderPath);
				_broker.Publish(new Folder { Id = folderId, Name = folderName }).Forget();
			}

			return folderId;
		}
	}
}
