using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
	public class FilePreExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly FileService _fileService;

		public FilePreExportWorker(WorkerQueueFactory workerQueueFactory, FileService fileService)
		{
			_workerQueueFactory = workerQueueFactory;
			_fileService = fileService;
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
							await _fileService.ProcessNewFile(processItem);
							outgoingQueue.Add(new FilePreExportQueueItem<File>(processItem));
						}
						else
						{
							var fileChange = await _fileService.ProcessChangedFile(processItem);
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

	}
}
