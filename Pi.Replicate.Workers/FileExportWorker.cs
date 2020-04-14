using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Events.SendFileToRecipient;
using Pi.Replicate.Domain;
using Serilog;
using System.Threading;

namespace Pi.Replicate.Workers
{
	public class FileExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;

		public FileExportWorker(IMediator mediator)
		{
			_mediator = mediator;
		}
		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var outgoingQueue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				while (!incomingQueue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var file = incomingQueue.Take();
					foreach (var recipient in file.Folder.Recipients)
					{
						Log.Information($"Sending metadata of file '{file.Path}' to '{recipient.Recipient.Name}'");
						await _mediator.Send(new SendFileToRecipientEvent { File = file, Recipient = recipient.Recipient });
					}
				}
			});

			thread.Start();
			return thread;
		}
	}
}