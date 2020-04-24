using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.FailedTransmissions.AddFailedTransmission;
using Pi.Replicate.Application.Folders.Queries.GetFolder;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
	public class FileExportWorker : WorkerBase
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
		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
				var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
				while (!incomingQueue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var file = incomingQueue.Take();
					var recipients = await _mediator.Send(new GetRecipientsForFolderQuery { FolderId = file.FolderId });
					var folder = await _mediator.Send(new GetFolderQuery { FolderId = file.FolderId });
					foreach (var recipient in recipients)
					{
						await _communicationService.SendFile(folder,file, recipient);
						outgoingQueue.Add(file);
					}
				}
			});

			thread.Start();
			return thread;
		}
	}
}