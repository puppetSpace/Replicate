﻿using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class WorkerNotificationProxy
	{
		private readonly string _workerHostAddress;

		public WorkerNotificationProxy(IConfiguration configuration)
		{
			_workerHostAddress = configuration[Constants.WorkerApiBaseAddressSetting];
		}

		public async Task PostFolderWebhookChanged(FolderWebhookChangedNotification folderWebhookChangeNotification)
		{
			using var channel = GrpcChannel.ForAddress(_workerHostAddress);

			try
			{
				var client = new Notifier.NotifierClient(channel);
				await client.FolderWebhookChangedAsync(new FolderWebhookRequest
				{
					FolderId = folderWebhookChangeNotification.FolderId.ToString(),
					WebHookType = folderWebhookChangeNotification.WebhookType,
					CallBackUrl = folderWebhookChangeNotification.CallbackUrl
				});
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to notify worker");
			}

		}

		public async Task PostRecipientsAdded(RecipientsAddedToFolderNotification recipientsAddedToFolderNotification)
		{
			using var channel = GrpcChannel.ForAddress(_workerHostAddress);

			try
			{
				var client = new Notifier.NotifierClient(channel);
				await client.RecipientAddedToFolderAsync(new RecipientAddedRequest
				{
					FolderId = recipientsAddedToFolderNotification.FolderId.ToString(),
					Recipients = { recipientsAddedToFolderNotification.Recipients.Select(x => x.ToString()) }
				});
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to notify worker");
			}
		}
	}
}
