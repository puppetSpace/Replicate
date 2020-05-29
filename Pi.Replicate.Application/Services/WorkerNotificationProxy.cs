using Microsoft.Extensions.Configuration;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Serilog;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class WorkerNotificationProxy
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _workerHostAddress;

		public WorkerNotificationProxy(IHttpClientFactory httpClientFactory, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_workerHostAddress = configuration[Constants.WorkerApiBaseAddressSetting];
		}

		public void PostFolderWebhookChanged(FolderWebhookChangedNotification folderWebhookChangeNotification)
		{
			PostChangeToWorker("folderwebhookchange",folderWebhookChangeNotification).Forget();
		}

		public void PostRecipientsAdded(RecipientsAddedToFolderNotification recipientsAddedToFolderNotification)
		{
			PostChangeToWorker("recipientsaddedtofolder", recipientsAddedToFolderNotification).Forget();
		}

		private async Task PostChangeToWorker(string action,object data)
		{
			try
			{
				Log.Information($"Executing action '{action}' on Workerhost");
				var client = _httpClientFactory.CreateClient("hostproxy");
				var response = await client.PostAsync($"{_workerHostAddress}/api/notification/{action}", data);
				if (!response.IsSuccessStatusCode)
					Log.Error($"Failed execute action '{action}'on Workerhost");
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error happend while executing action '{action}' on Workerhost");
			}
		}
	}
}
