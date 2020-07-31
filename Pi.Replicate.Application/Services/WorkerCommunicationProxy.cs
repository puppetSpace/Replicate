using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Models;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class WorkerCommunicationProxy
	{
		private static GrpcChannel _channel;

		public WorkerCommunicationProxy(IConfiguration configuration)
		{
			_channel = GrpcChannel.ForAddress(configuration[Constants.WorkerApiBaseAddressSetting]);
		}

		public async Task PostFolderWebhookChanged(FolderWebhookChangedNotification folderWebhookChangeNotification)
		{
			try
			{
				var client = new Communicator.CommunicatorClient(_channel);
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
			try
			{
				var client = new Communicator.CommunicatorClient(_channel);
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

		public async Task<(string hostname, string exception)> ProbeRecipient(string address)
		{
			using var channel =  GrpcChannel.ForAddress(address);
			try
			{
				var client = new Communicator.CommunicatorClient(channel);
				var result = await client.ProbeRecipientAsync(new ProbeRequest());
				return (result.MachineName,null);
			}
			catch(Exception ex)
			{
				Log.Error(ex, $"Failed to probe {address}");
				return (null,ex.Message);
			}
		}

		public async Task<string> GetBaseFolder()
		{
			try
			{
				var client = new Communicator.CommunicatorClient(_channel);
				var response = await client.GetBaseFolderAsync(new BaseFolderRequest());
				return response?.Path;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to retrieve BaseFolderPath");
				return null;
			}
		}
	}
}
