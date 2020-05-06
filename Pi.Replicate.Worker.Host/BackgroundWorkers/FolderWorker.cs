using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FolderWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly IMediator _mediator;
		private readonly FileCollectorFactory _fileCollectorFactory;
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly FileService _fileService;

		public FolderWorker(IConfiguration configuration
			, IMediator mediator
			, FileCollectorFactory fileCollectorFactory
			, WorkerQueueFactory workerQueueFactory
			, FileService fileService)
		{
			_triggerInterval = int.Parse(configuration[Constants.FolderCrawlTriggerInterval]);
			_mediator = mediator;
			_fileCollectorFactory = fileCollectorFactory;
			_workerQueueFactory = workerQueueFactory;
			_fileService = fileService;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new System.Threading.Thread(async() =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					var result = await _mediator.Send(new GetFoldersToCrawlQuery(), stoppingToken);
					if (result.WasSuccessful)
					{
						foreach (var folder in result.Data)
						{
							try
							{
								Log.Information($"Crawling through folder '{folder.Name}'");
								var collector = _fileCollectorFactory.Get(folder);
								await collector.CollectFiles();
								await ProcessNewFiles(folder, collector.NewFiles);
								await ProcessChangedFiles(folder, collector.ChangedFiles);
							}
							catch (Exception ex)
							{
								Log.Error(ex, $"Unexpected error occured during processing of folder {folder.Name}");
							}
						}
					}

					Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min for next cycle of foldercrawling");
					await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
				}
			});
			th.Start();

			return Task.CompletedTask;
		}

		private async Task ProcessNewFiles(Folder folder, List<System.IO.FileInfo> newFiles)
		{
			var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
			foreach (var newFile in newFiles)
			{
				var createdFile = await _fileService.CreateNewFile(folder, newFile);
				Log.Debug($"Adding '{createdFile.Path}' to queue");
				if (queue.Any(x => string.Equals(x.Path, createdFile.Path)))
					Log.Information($"{createdFile.Path} already present in queue for processing");
				else
					queue.Add(createdFile);
			}
		}

		private async Task ProcessChangedFiles(Folder folder, List<System.IO.FileInfo> changedFiles)
		{
			var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
			foreach (var changedFile in changedFiles)
			{
				var updatedFile = await _fileService.CreateUpdateFile(folder, changedFile);
				if (updatedFile is object)
				{
					Log.Debug($"Adding '{updatedFile.Path}' to queue");
					if (queue.Any(x => string.Equals(x.Path, updatedFile.Path)))
						Log.Information($"{updatedFile.Path} already present in queue for processing");
					else
						queue.Add(updatedFile);
				}
				else
				{
					Log.Information($"Unable to perform update action. No existing file for '{changedFile.FullName}'");
				}
			}
		}


	}
}
