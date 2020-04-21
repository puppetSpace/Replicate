using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.FileChanges.Commands.AddFileChange;
using Pi.Replicate.Application.FileChanges.Queries.GetHighestChangeVersionNo;
using Pi.Replicate.Application.Files.Commands.UpdateFile;
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
		private readonly ChunkService _chunkService;

		public FilePreExportWorker(IConfiguration configuration
		, WorkerQueueFactory workerQueueFactory
		, IMediator mediator
		, PathBuilder pathBuilder
		, ChunkService chunkService)
		{
			_workerQueueFactory = workerQueueFactory;
			_mediator = mediator;
			_pathBuilder = pathBuilder;
			_chunkService = chunkService;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(FilePreExportWorker)}");
				var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
				var outgoingQueue = _workerQueueFactory.Get<FilePreExportQueueItem>(WorkerQueueType.ToSendFiles);
				var runningTasks = new List<Task>();
				var semaphore = new SemaphoreSlim(10); //todo create setting for this
				while (!incomingQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
				{
					runningTasks.RemoveAll(x => x.IsCompleted);
					var processItem = incomingQueue.Take(cancellationToken);
					runningTasks.Add(Task.Run(async () =>
					{
						await semaphore.WaitAsync();
						Log.Information($"'{processItem.Path}' is being processed");
						if (processItem.Status == FileStatus.New)
						{
							await ProcessNewItem(processItem);
							outgoingQueue.Add(new FilePreExportQueueItem<File>(processItem));
						}
						else
						{
							var fileChange = await ProcessChangedItem(processItem);
							outgoingQueue.Add(new FilePreExportQueueItem<FileChange>(fileChange));
						}
						Log.Information($"'{processItem.Path}' is processed");
						semaphore.Release();
					}));

				}
				await Task.WhenAll(runningTasks);
			});

			thread.Start();
			return thread;
		}

		private async Task ProcessNewItem(File processItem)
		{
			var amountOfChunks = await _chunkService.SplitFileIntoChunks(processItem);
			processItem.SetAmountOfChunks(amountOfChunks);
			processItem.MarkAsProcessed();
			await _mediator.Send(new UpdateFileCommand { File = processItem });
		}

		private async Task<FileChange> ProcessChangedItem(File processItem)
		{
			//todo refactor
			var path = _pathBuilder.BuildPath(processItem.Path);
			var deltaservice = new DeltaService();
			var delta = deltaservice.CreateDelta(path, processItem.Signature);
			var newSignature = deltaservice.CreateSignature(path);
			var highestVersionNo = await _mediator.Send(new GetHighestChangeVersionNoQuery { FileId = processItem.Id });
			highestVersionNo += 1;
			var amountOfChunks = await _chunkService.SplitMemoryIntoChunks(processItem, highestVersionNo, delta);
			var fileChange = FileChange.Build(processItem, highestVersionNo, amountOfChunks, processItem.Signature, processItem.LastModifiedDate);
			await _mediator.Send(new AddFileChangeCommand { FileChange = fileChange });

			processItem.SetSignature(newSignature);
			processItem.MarkAsProcessed();
			await _mediator.Send(new UpdateFileCommand { File = processItem, AlsoUpdateSignature = true });
			return fileChange;

		}
	}
}
