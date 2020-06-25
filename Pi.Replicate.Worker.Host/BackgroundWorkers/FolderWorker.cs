using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FolderWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly FolderRepository _folderRespository;
		private readonly FileCollectorFactory _fileCollectorFactory;
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly FileService _fileService;

		public FolderWorker(IConfiguration configuration
			, FolderRepository folderRespository
			, FileCollectorFactory fileCollectorFactory
			, WorkerQueueContainer workerQueueContainer
			, FileService fileService)
		{
			_triggerInterval = int.Parse(configuration[Constants.FolderCrawlTriggerInterval]);
			_folderRespository = folderRespository;
			_fileCollectorFactory = fileCollectorFactory;
			_workerQueueContainer = workerQueueContainer;
			_fileService = fileService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new System.Threading.Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(FolderWorker)}");
				while (!stoppingToken.IsCancellationRequested)
				{
					var queryResult = await _folderRespository.GetFoldersToCrawl();
					if (queryResult.WasSuccessful)
					{
						foreach (var folder in queryResult.Data)
						{
							try
							{
								WorkerLog.Instance.Information($"Crawling through folder '{folder.Name}'");
								var collector = _fileCollectorFactory.Get(folder);
								await collector.CollectFiles();
								await ProcessNewFiles(folder, collector.NewFiles);
								await ProcessChangedFiles(folder, collector.ChangedFiles);
							}
							catch (Exception ex)
							{
								WorkerLog.Instance.Error(ex, $"Unexpected error occured during processing of folder {folder.Name}");
							}
						}
					}
					WorkerLog.Instance.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min for next cycle of foldercrawling");
					await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
				}
			});
			th.Start();

			await Task.Delay(Timeout.Infinite);
		}

		private async Task ProcessNewFiles(CrawledFolder folder, List<System.IO.FileInfo> newFiles)
		{
			var queue = _workerQueueContainer.ToSendFiles.Writer;
			foreach (var newFile in newFiles)
			{
				var createdFile = await _fileService.CreateNewFile(folder.Id, newFile);
				if (createdFile is object)
				{
					WorkerLog.Instance.Debug($"Adding '{createdFile.Path}' to queue");
					if (await queue.WaitToWriteAsync())
						await queue.WriteAsync(createdFile);
				}
			}
		}

		private async Task ProcessChangedFiles(CrawledFolder folder, List<System.IO.FileInfo> changedFiles)
		{
			var queue = _workerQueueContainer.ToSendFiles.Writer;
			foreach (var changedFile in changedFiles)
			{
				var updatedFile = await _fileService.CreateUpdateFile(folder.Id, changedFile);
				if (updatedFile is object)
				{
					if (await queue.WaitToWriteAsync())
						await queue.WriteAsync(updatedFile);
				}
			}
		}


	}
}
