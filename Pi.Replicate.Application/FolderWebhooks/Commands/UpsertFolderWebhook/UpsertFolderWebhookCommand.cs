using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Shared.Models;
using System;
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
		private readonly WorkerNotificationProxy _workerProxy;
		private const string _upsertStatement = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.FolderWebhook WHERE FolderId = @FolderId AND WebhookTypeId = @WebhookTypeId)
				INSERT INTO dbo.FolderWebhook(Id,FolderId,WebhookTypeId,CallbackUrl) VALUES(NEWID(),@FolderId,@WebhookTypeId,@CallbackUrl)
			ELSE
				UPDATE dbo.FolderWebhook SET CallbackUrl = @CallbackUrl WHERE FolderId = @FolderId AND WebhookTypeId = @WebhookTypeId";

		public UpsertFolderWebhookCommandHandler(IDatabase database, WorkerNotificationProxy workerProxy)
		{
			_database = database;
			_workerProxy = workerProxy;
		}

		public async Task<Result> Handle(UpsertFolderWebhookCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				await _database.Execute(_upsertStatement, new { request.FolderId, request.WebhookTypeId, request.CallbackUrl });
				_workerProxy.PostFolderWebhookChanged(new FolderWebhookChangedNotification { FolderId = request.FolderId, WebhookType = request.WebhookTypeName });
				return Result.Success();
			}
		}
	}
}
