using Microsoft.Extensions.Hosting;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Services;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileChunkExportWorker : BackgroundService
	{
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly TransmissionService _transmissionService;

		public FileChunkExportWorker(WorkerQueueContainer workerQueueContainer, TransmissionService transmissionService)
		{
			_workerQueueContainer = workerQueueContainer;
			_transmissionService = transmissionService;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(FileChunkExportWorker)}");
				var queue = _workerQueueContainer.ToSendChunks.Reader;
				while (await queue.WaitToReadAsync() || !stoppingToken.IsCancellationRequested)
				{
					var chunkPackage = await queue.ReadAsync();
					await _transmissionService.SendFileChunk(chunkPackage.filechunk, chunkPackage.recipient);
				}
			});
			th.Start();

			await Task.Delay(Timeout.Infinite);
		}
	}
}
