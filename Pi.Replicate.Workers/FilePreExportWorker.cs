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
	public class FilePreExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;
		private readonly PathBuilder _pathBuilder;
		private readonly ChunkService _fileSplitterService;

		public FilePreExportWorker(IConfiguration configuration
		, WorkerQueueFactory workerQueueFactory
		, IMediator mediator
		, PathBuilder pathBuilder
		, ChunkService fileSplitterService)
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
				Log.Information($"Starting {nameof(Replicate.Workers.FilePreExportWorker)}");
				var incomingQueue = _workerQueueFactory.Get<ProcessItem<File, FolderOption>>((WorkerQueueType)WorkerQueueType.ToProcessFiles);
				var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var runningTasks = new List<Task>();
				var semaphore = new SemaphoreSlim(10); //todo create setting for this
				while (!incomingQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
				{
					runningTasks.RemoveAll(x => x.IsCompleted);
					var processItem = incomingQueue.Take(cancellationToken);
					runningTasks.Add(Task.Run(async () =>
					{
						await semaphore.WaitAsync();
						Log.Information($"'{processItem.Item.Path}' is being processed");
						if (processItem.Item.Status == FileStatus.New)
						{
							await ProcessNewItem(processItem);
						}
						else
						{
							await ProcessChangedItem(processItem);
						}
						outgoingQueue.Add(processItem.Item);
						Log.Information($"'{processItem.Item.Path}' is processed");
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
			var amountOfChunks = await _fileSplitterService.SplitFileIntoChunksAndProduceHash(processItem.Item);
//move delta to splitfile service???
			var delta = new Delta();
			var signature = delta.CreateSignature(_pathBuilder.BuildPath(processItem.Item.Path));
			await _mediator.Send(new UpdateFileAsProcessedCommand { File = processItem.Item, Signature = signature, AmountOfChunks = amountOfChunks });

			if (processItem.Metadata.DeleteAfterSent)
			{
				var path = _pathBuilder.BuildPath(processItem.Item.Path);
				System.IO.File.Delete(path);
			}
		}

		private async Task ProcessChangedItem(ProcessItem<File, FolderOption> processItem)
		{
			//todo maybe don't use hash???
			var path = _pathBuilder.BuildPath(processItem.Item.Path);
			var delta = new Delta(); //todo create service of this
			var deltaChange = delta.CreateDelta(path,processItem.Item.Signature);
			var newSignature = delta.CreateSignature(path);

			//todo split up deltaChange into chunks

			//await _mediator.Send(new UpdateFileAsProcessedCommand { File = processItem.Item, Signature = newSignature, AmountOfChunks = result.amountOfChunks });
			
		}
	}
}
