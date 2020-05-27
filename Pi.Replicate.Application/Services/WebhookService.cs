using MediatR;
using Observr;
using Pi.Replicate.Application.Folders.Queries.GetFolder;
using Pi.Replicate.Application.FolderWebhooks.Notifications.FolderWebhookChanged;
using Pi.Replicate.Application.FolderWebhooks.Queries.GetFolderWebhooks;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
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

		private readonly IMediator _mediator;
		private readonly PathBuilder _pathBuilder;
		private readonly IHttpClientFactory _httpClientFactory;
		private List<WebhookCacheItem> _webhookCache = new List<WebhookCacheItem>();

		public WebhookService(IMediator mediator, PathBuilder pathBuilder, IHttpClientFactory httpClientFactory, IBroker broker)
		{
			_mediator = mediator;
			_pathBuilder = pathBuilder;
			_httpClientFactory = httpClientFactory;
			broker.Subscribe(this);
		}

		public Task Handle(FolderWebhookChangedNotification value, CancellationToken cancellationToken)
		{
			if (value is null)
				return Task.CompletedTask;

			var found = _webhookCache.FirstOrDefault(x => x.FolderId == value.FolderId && string.Equals(x.WebhookTypeName, value.WebhookType));
			if (found is object)
			{
				found.CallbackUrl = value.CallbackUrl;
				Log.Debug($"Webhook of type '{value.WebhookType}' for folderId '{value.FolderId}' updated callbackUrl to '{value.CallbackUrl}' ");
			}
			else
			{
				_webhookCache.Add(new WebhookCacheItem { FolderId = value.FolderId, WebhookTypeName = value.WebhookType, CallbackUrl = value.CallbackUrl });
				Log.Debug($"Webhook of type '{value.WebhookType}' for folderId '{value.FolderId}' added with callbackUrl '{value.CallbackUrl}' ");
			}

			return Task.CompletedTask;
		}

		public async Task Initialize()
		{
			var result = await _mediator.Send(new GetFolderWebhooksQuery { });
			if (result.WasSuccessful)
			{
				_webhookCache = result.Data.Select(x => new WebhookCacheItem { FolderId = x.FolderId, WebhookTypeName = x.WebhookType.Name, CallbackUrl = x.CallbackUrl }).ToList();
			}
		}

		public void NotifyFileAssembled(File file) => CallWebhook(file, _typeFileAssembled).Forget();

		public void NotifyFileDisassembled(File file) => CallWebhook(file, _typeFileDisassembled).Forget();

		public void NotifyFileFailed(File file) => CallWebhook(file, _typeFileFailed).Forget();

		private async Task CallWebhook(File file, string type)
		{
			var foundWebhook = _webhookCache.FirstOrDefault(x => string.Equals(x.WebhookTypeName, type) && x.FolderId == file.FolderId);
			var foundFolder = await _mediator.Send(new GetFolderQuery { FolderId = file.FolderId });
			if (foundWebhook is object && !string.IsNullOrWhiteSpace(foundWebhook.CallbackUrl))
			{
				var postObject = new { name = file.Name, path = _pathBuilder.BuildPath(file.Path), version = file.Version, folder = foundFolder?.Data?.Name };

				Log.Debug($"Webhook of type '{type}' for folder '{foundFolder?.Data?.Name}' calling '{foundWebhook.CallbackUrl}'");
				var httpClient = _httpClientFactory.CreateClient("webhook");
				try
				{
					await httpClient.PostAsync(foundWebhook.CallbackUrl, (object)postObject, throwErrorOnResponseNok: true);
				}
				catch (Exception ex)
				{
					Log.Error(ex.InnerException, $"Failed to call webhook of type '{type}' for folder '{foundFolder?.Data?.Name}'");
				}
			}
			else
			{
				Log.Warning($"No webhook found of type '{type}' for folder '{foundFolder?.Data?.Name}'");
			}

		}

		private class WebhookCacheItem
		{
			public Guid FolderId { get; set; }

			public string WebhookTypeName { get; set; }

			public string CallbackUrl { get; set; }
		}

	}
}
