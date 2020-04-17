using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Commands.UpdateFileAsProcessed;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
	public class FileProcessForExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;
		private readonly PathBuilder _pathBuilder;
		private readonly FileChunkService _fileSplitterService;

		public FileProcessForExportWorker(IConfiguration configuration
		, WorkerQueueFactory workerQueueFactory
		, IMediator mediator
		, PathBuilder pathBuilder
		, FileChunkService fileSplitterService)
		{
			_workerQueueFactory = workerQueueFactory;
			_mediator = mediator;
			_pathBuilder = pathBuilder;
			_fileSplitterService = fileSplitterService;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(Replicate.Workers.FileProcessForExportWorker)}");
				var incomingQueue = _workerQueueFactory.Get<ProcessItem<File, FolderOption>>((WorkerQueueType)WorkerQueueType.ToProcessFiles);
				var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var runningTasks = new List<Task>();
				var semaphore = new SemaphoreSlim(10); //todo create setting for this
				while (!incomingQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
				{
					runningTasks.RemoveAll(x => (bool)x.IsCompleted);
					var processItem = incomingQueue.Take(cancellationToken);
					runningTasks.Add(Task.Run(async () =>
					{
						await semaphore.WaitAsync();
						if (processItem.Item.Status == FileStatus.New)
						{
							await ProcessNewItem(processItem);
						}
						else
						{
							//todo process update
						}
						outgoingQueue.Add(processItem.Item);

						semaphore.Release();
					}));

				}
				await Task.WhenAll(runningTasks);
			});

			thread.Start();
			return thread;
		}

		private async Task ProcessNewItem(ProcessItem<File, FolderOption> processItem)
		{
			var result = await _fileSplitterService.SplitFileIntoChunksAndProduceHash(processItem.Item);

			var delta = new Delta();
			var signature = delta.CreateSignature(_pathBuilder.BuildPath(processItem.Item.Path));
			await _mediator.Send(new UpdateFileAsProcessedCommand { File = processItem.Item, Hash = result.fileHash, Signature = signature, AmountOfChunks = result.amountOfChunks });

			if (processItem.Metadata.DeleteAfterSent)
			{
				var path = _pathBuilder.BuildPath(processItem.Item.Path);
				System.IO.File.Delete(path);
			}
		}
	}
}
