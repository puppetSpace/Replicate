using MediatR;
using Microsoft.Extensions.Configuration;
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

namespace Pi.Replicate.Workers
{
	public class FolderWorker : WorkerBase
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
			_triggerInterval = int.TryParse(configuration[Constants.FolderCrawlTriggerInterval], out int interval) ? interval : 10;
			_mediator = mediator;
			_fileCollectorFactory = fileCollectorFactory;
			_workerQueueFactory = workerQueueFactory;
			_fileService = fileService;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var workingThread = new Thread(async () =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					var folders = await _mediator.Send(new GetFoldersToCrawlQuery(), cancellationToken);
					foreach (var folder in folders)
					{
						Log.Information($"Crawling through folder '{folder.Name}'");
						var collector = _fileCollectorFactory.Get(folder);
						await collector.CollectFiles();
						await ProcessNewFiles(folder, collector.NewFiles);
						await ProcessChangedFiles(folder, collector.ChangedFiles);
					}

					Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min for next cycle of foldercrawling");
					await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
				}

			});
			workingThread.Start();
			return workingThread;
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
