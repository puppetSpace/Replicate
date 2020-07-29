using Microsoft.Extensions.Hosting;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileChunkReceiverWorker : BackgroundService
	{
		private readonly IFileChunkRepository _fileChunkRepository;
		private readonly WorkerQueueContainer _workerQueueContainer;

		public FileChunkReceiverWorker(IFileChunkRepository fileChunkRepository, WorkerQueueContainer workerQueueContainer)
		{
			_fileChunkRepository = fileChunkRepository;
			_workerQueueContainer = workerQueueContainer;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var thread = new Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(FileChunkReceiverWorker)}");
				var incomingQueue = _workerQueueContainer.ReceivedChunks.Reader;
				while (await incomingQueue.WaitToReadAsync() || !stoppingToken.IsCancellationRequested)
				{
					var fileChunk = await incomingQueue.ReadAsync();
					await _fileChunkRepository.AddReceivedFileChunk(fileChunk);
				}
			});

			thread.Start();
			await Task.Delay(Timeout.Infinite);
		}
	}
}
