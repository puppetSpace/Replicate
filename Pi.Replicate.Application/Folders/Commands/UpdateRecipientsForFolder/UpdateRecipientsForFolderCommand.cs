using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Folders.Notifications.RecipientsAddedToFolder;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.AddRecipientToFolder
{
    public class UpdateRecipientsForFolderCommand : IRequest<Result>
    {
		public Guid FolderId { get; set; }

		public List<Recipient> ToAddRecipients { get; set; }

		public List<Recipient> ToDeleteRecipients { get; set; }
	}

	public class UpdateRecipientsForFolderCommandHandler : IRequestHandler<UpdateRecipientsForFolderCommand, Result>
	{
		private readonly IDatabase _database;
		private readonly IMediator _mediator;
		private const string _insertStatement = "INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,@RecipientId)";
		private const string _deleteStatement = "DELETE FROM dbo.FolderRecipient WHERE FolderId = @FolderId AND RecipientId = @RecipientId";


		public UpdateRecipientsForFolderCommandHandler(IDatabase database, IMediator mediator)
		{
			_database = database;
			_mediator = mediator;
		}

		public async Task<Result> Handle(UpdateRecipientsForFolderCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				foreach (var toAddRecipient in request.ToAddRecipients)
					await _database.Execute(_insertStatement, new { request.FolderId, RecipientId = toAddRecipient.Id });

				foreach (var toDeleteRecipient in request.ToDeleteRecipients)
					await _database.Execute(_deleteStatement, new { request.FolderId, RecipientId = toDeleteRecipient.Id });

				_mediator.Publish(new RecipientsAddedToFolderNotification { FolderId = request.FolderId, Recipients = request.ToAddRecipients }).Forget(); //can take a while if queue is full
				return Result.Success();
			}
		}
	}
}
