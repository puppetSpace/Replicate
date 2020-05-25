using MediatR;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Folders.Queries.GetFolder;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	//todo retry mechanisme in database
	public class FileExportWorker : BackgroundService
	{
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly IMediator _mediator;
		private readonly TransmissionService _communicationService;

		public FileExportWorker(IMediator mediator, TransmissionService communicationService, WorkerQueueContainer workerQueueContainer)
		{
			_mediator = mediator;
			_communicationService = communicationService;
			_workerQueueContainer = workerQueueContainer;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(FileExportWorker)}");
				var incomingQueue = _workerQueueContainer.ToSendFiles.Reader;
				var outgoingQueue = _workerQueueContainer.ToProcessFiles.Writer;
				while (await incomingQueue.WaitToReadAsync() || !stoppingToken.IsCancellationRequested)
				{
					var file = await incomingQueue.ReadAsync();
					var folderResult = await _mediator.Send(new GetFolderQuery { FolderId = file.FolderId });

					if (folderResult.WasSuccessful)
					{
						var recipients = file is RequestFile rf ? rf.Recipients : folderResult.Data.Recipients;
						foreach (var recipient in recipients)
							await _communicationService.SendFile(folderResult.Data, file, recipient);

						if (await outgoingQueue.WaitToWriteAsync())
							await outgoingQueue.WriteAsync(file);
					}
				}
			});

			th.Start();

			return Task.CompletedTask;
		}
	}
}