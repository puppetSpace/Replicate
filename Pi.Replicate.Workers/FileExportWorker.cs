using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using System.Threading;

namespace Pi.Replicate.Workers
{
	public class FileExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;
		private readonly CommunicationService _communicationService;

		public FileExportWorker(IMediator mediator, CommunicationService communicationService, WorkerQueueFactory workerQueueFactory)
		{
			_mediator = mediator;
			_communicationService = communicationService;
			_workerQueueFactory = workerQueueFactory;
		}
		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var outgoingQueue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				var sendFileHandler = new SendFileHandler(_mediator, _communicationService);
				while (!incomingQueue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var file = incomingQueue.Take();
					var recipients = await _mediator.Send(new GetRecipientsForFolderQuery { FolderId = file.FolderId });
					foreach (var recipient in recipients)
					{
						//todo try todo this in parallel
						await sendFileHandler.Handle(file, recipient, outgoingQueue);
					}
				}
			});

			thread.Start();
			return thread;
		}
	}
}