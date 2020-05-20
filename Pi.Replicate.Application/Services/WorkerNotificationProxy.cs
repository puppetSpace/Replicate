using Pi.Replicate.Application.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pi.Replicate.Shared;
using Microsoft.Extensions.Configuration;
using Serilog;

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

		public void PostFolderWebhookChanged(FolderWebhookChangeNotification folderWebhookChangeNotification)
		{
			PostFolderWebhookChangedTask(folderWebhookChangeNotification).Forget();
		}

		private async Task PostFolderWebhookChangedTask(FolderWebhookChangeNotification folderWebhookChangeNotification)
		{
			try
			{
				Log.Information("Notifying Workerhost for changes to a FolderWebhook");
				var client = _httpClientFactory.CreateClient("hostproxy");
				var response = await client.PostAsync($"{_workerHostAddress}/api/notification/folderwebhookchange", folderWebhookChangeNotification);
				if (!response.IsSuccessStatusCode)
					Log.Error("Failed to contact Workerhost for notifying changes to FolderWebhook");
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Error happend while notifying workerhost of FolderWebhook change");
			}
		}
	}
}
