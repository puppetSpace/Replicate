using MediatR;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Queries.GetSignatureOfFile;
using Pi.Replicate.Application.Folders.Queries.GetFolder;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileExportWorker : BackgroundService
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;
		private readonly TransmissionService _communicationService;

		public FileExportWorker(IMediator mediator, TransmissionService communicationService, WorkerQueueFactory workerQueueFactory)
		{
			_mediator = mediator;
			_communicationService = communicationService;
			_workerQueueFactory = workerQueueFactory;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
				while (!incomingQueue.IsCompleted || !stoppingToken.IsCancellationRequested)
				{
					var file = incomingQueue.Take(); //since no task has been awaited, this blocks the main thread. So run inside of task
					var folderResult = await _mediator.Send(new GetFolderQuery { FolderId = file.FolderId });
					var signatureResult = await _mediator.Send(new GetSignatureOfFileQuery { FileId = file.Id });
					if (folderResult.WasSuccessful && signatureResult.WasSuccessful)
					{
						foreach (var recipient in folderResult.Data.Recipients)
						{
							if(await _communicationService.SendFile(folderResult.Data, file, signatureResult.Data, recipient))
								outgoingQueue.Add(file);
						}
					}
				}
			});

			th.Start();

			return Task.CompletedTask;
		}
	}
}