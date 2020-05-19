using MediatR;
using Observr;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Notifications.Models;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FolderWebhooks.Commands.UpsertFolderWebhook
{
    public class UpsertFolderWebhookCommand : IRequest<Result>
    {
		public Guid FolderId { get; set; }

		public Guid WebhookTypeId { get; set; }

		public string WebhookTypeName { get; set; }

		public string CallbackUrl { get; set; }
	}

	public class UpsertFolderWebhookCommandHandler : IRequestHandler<UpsertFolderWebhookCommand, Result>
	{
		private readonly IDatabase _database;
		private readonly IBroker _broker;
		private const string _upsertStatement = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.FolderWebhook WHERE FolderId = @FolderId AND WebhookTypeId = @WebhookTypeId)
				INSERT INTO dbo.FolderWebhook(Id,FolderId,WebhookTypeId,CallbackUrl) VALUES(NEWID(),@FolderId,@WebhookTypeId,@CallbackUrl)
			ELSE
				UPDATE dbo.FolderWebhook SET CallbackUrl = @CallbackUrl WHERE FolderId = @FolderId AND WebhookTypeId = @WebhookTypeId";

		public UpsertFolderWebhookCommandHandler(IDatabase database, IBroker broker)
		{
			_database = database;
			_broker = broker;
		}

		public async Task<Result> Handle(UpsertFolderWebhookCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				await _database.Execute(_upsertStatement, new { request.FolderId, request.WebhookTypeId, request.CallbackUrl });
				await _broker.Publish(new FolderWebhookChangeNotification { FolderId = request.FolderId, WebhookType = request.WebhookTypeName });
				return Result.Success();
			}
		}
	}
}
