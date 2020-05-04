using MediatR;
using Observr;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
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

        public string SenderAddress { get; set; }
    }

    public class AddReceivedFolderCommandHandler : IRequestHandler<AddReceivedFolderCommand, Guid>
    {
        private const string _folderInsertStatement = @"INSERT INTO dbo.Folder(Id,Name) VALUES(@Id,@Name)";
        private const string _folderIdSelectStatement = "SELECT Id FROM dbo.Folder WHERE Name = @Name";
		private const string _recipientCreationStatement = @"
			BEGIN
				DECLARE @recipientId uniqueidentifier;

				SELECT @recipientId = Id
				FROM dbo.Recipient
				WHERE [Address] = @Address;

				IF(@recipientId is null)
				BEGIN
					SET @recipientId = NEWID();
					INSERT INTO dbo.Recipient(Id,[Name],[Address], Verified) VALUES(@recipientId,@Name,@Address,1);
				END

				IF NOT EXISTS (SELECT 1 FROM dbo.FolderRecipient WHERE FolderId = @FolderId and RecipientId = @recipientId)
					INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,@recipientId)
			END";
        private readonly IDatabase _database;
        private readonly PathBuilder _pathBuilder;
		private readonly IBroker _broker;

		public AddReceivedFolderCommandHandler(IDatabase database, PathBuilder pathBuilder, IBroker broker)
        {
            _database = database;
            _pathBuilder = pathBuilder;
			_broker = broker;
		}

        public async Task<Guid> Handle(AddReceivedFolderCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                Guid folderId = await CreateFolderIfNotExists(request);
                await CreateRecipientOfNotExists(request,folderId);
                return folderId;
            }
        }

        private async Task<Guid> CreateFolderIfNotExists(AddReceivedFolderCommand request)
        {
            var folderId = await _database.QuerySingle<Guid>(_folderIdSelectStatement, new { request.Name });
            if (folderId == Guid.Empty)
            {
				Log.Verbose($"Creating new folder {request.Name}");
                folderId = Guid.NewGuid();
                await _database.Execute(_folderInsertStatement, new { Id = folderId, request.Name });
                var folderPath = _pathBuilder.BuildPath(request.Name);
                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);
				_broker.Publish(new Folder { Id = folderId, Name = request.Name }).Forget();
            }

            return folderId;
        }

        private async Task CreateRecipientOfNotExists(AddReceivedFolderCommand request, Guid folderId)
        {
            await _database.Execute(_recipientCreationStatement,new {Address = request.SenderAddress,Name = request.Sender,FolderId = folderId});
        }
    }
}
