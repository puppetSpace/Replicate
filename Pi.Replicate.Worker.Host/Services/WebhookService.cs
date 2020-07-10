using Observr;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public interface IWebhookService
	{
		Task Initialize();
		void NotifyFileAssembled(File file);
		void NotifyFileDisassembled(File file);
		void NotifyFileFailed(File file);
	}


	public class WebhookService : Observr.IObserver<FolderWebhookChangedNotification>, IWebhookService
	{
		private const string _typeFileAssembled = "FileAssembled";
		private const string _typeFileDisassembled = "FileDisassembled";
		private const string _typeFileFailed = "FileFailed";

		private readonly IHttpClientFactory _httpClientFactory;
		private readonly FolderRepository _folderRepository;
		private readonly WebhookRepository _webhookRepository;
		private List<Webhook> _webhookCache = new List<Webhook>();

		public WebhookService( IHttpClientFactory httpClientFactory
			, IBroker broker
			, FolderRepository folderRepository
			, WebhookRepository webhookRepository
			)
		{
			_httpClientFactory = httpClientFactory;
			_folderRepository = folderRepository;
			_webhookRepository = webhookRepository;
			broker.Subscribe(this);
		}

		public Task Handle(FolderWebhookChangedNotification value, CancellationToken cancellationToken)
		{
			if (value is null)
				return Task.CompletedTask;

			var found = _webhookCache.FirstOrDefault(x => x.FolderId == value.FolderId && string.Equals(x.Type, value.WebhookType));
			if (found is object)
			{
				found.CallbackUrl = value.CallbackUrl;
				WorkerLog.Instance.Debug($"Webhook of type '{value.WebhookType}' for folderId '{value.FolderId}' updated callbackUrl to '{value.CallbackUrl}' ");
			}
			else
			{
				_webhookCache.Add(new Webhook { FolderId = value.FolderId, Type = value.WebhookType, CallbackUrl = value.CallbackUrl });
				WorkerLog.Instance.Debug($"Webhook of type '{value.WebhookType}' for folderId '{value.FolderId}' added with callbackUrl '{value.CallbackUrl}' ");
			}

			return Task.CompletedTask;
		}

		public async Task Initialize()
		{
			var result = await _webhookRepository.GetWebhooks();
			if (result.WasSuccessful)
			{
				_webhookCache = result.Data.ToList();
			}
		}

		public void NotifyFileAssembled(File file) => CallWebhook(file, _typeFileAssembled).Forget();

		public void NotifyFileDisassembled(File file) => CallWebhook(file, _typeFileDisassembled).Forget();

		public void NotifyFileFailed(File file) => CallWebhook(file, _typeFileFailed).Forget();

		private async Task CallWebhook(File file, string type)
		{
			var foundWebhook = _webhookCache.FirstOrDefault(x => string.Equals(x.Type, type) && x.FolderId == file.FolderId);
			var foundFolder = await _folderRepository.GetFolderName(file.FolderId);
			var folderName = foundFolder?.Data;
			if (foundWebhook is object && !string.IsNullOrWhiteSpace(foundWebhook.CallbackUrl))
			{
				var postObject = new { name = file.Name, path = PathBuilder.BuildPath(file.Path), version = file.Version, folder = folderName };

				WorkerLog.Instance.Debug($"Webhook of type '{type}' for folder '{folderName}' calling '{foundWebhook.CallbackUrl}'");
				var httpClient = _httpClientFactory.CreateClient("webhook");
				try
				{
					await httpClient.PostAsync(foundWebhook.CallbackUrl, (object)postObject, throwErrorOnResponseNok: true);
				}
				catch (Exception ex)
				{
					WorkerLog.Instance.Error(ex.InnerException, $"Failed to call webhook of type '{type}' for folder '{folderName}'");
				}
			}
			else
			{
				WorkerLog.Instance.Warning($"No webhook found of type '{type}' for folder '{folderName}'");
			}

		}

	}
}
