using MediatR;
using Pi.Replicate.Application.Common.Queues;
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
				var incomingQueue = _workerQueueFactory.Get<FilePreExportQueueItem>(WorkerQueueType.ToSendFiles);
				var outgoingQueue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				var sendFileHandler = new SendFileHandler(_mediator, _communicationService);
				while (!incomingQueue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var queueItem = incomingQueue.Take();
					File file = null;
					FileChange fileChange = null;
					if (queueItem is FilePreExportQueueItem<File> fileItem)
					{
						file = fileItem.Item;
					}
					else if (queueItem is FilePreExportQueueItem<FileChange> fileChangeItem)
					{
						file = fileChangeItem.Item.File;
						fileChange = fileChangeItem.Item;
					}

					var recipients = await _mediator.Send(new GetRecipientsForFolderQuery { FolderId = file.FolderId });
					if (fileChange is null)
						await HandleNewFile(sendFileHandler,file, recipients, outgoingQueue);
					else
						await HandleFileChange(sendFileHandler, fileChange, recipients, outgoingQueue);
					


				}
			});

			thread.Start();
			return thread;
		}

		private async Task HandleFileChange(SendFileHandler sendFileHandler, FileChange fileChange, ICollection<Recipient> recipients, BlockingCollection<ChunkPackage> outgoingQueue)
		{
			foreach (var recipient in recipients)
			{
				//todo try todo this in parallel
				await sendFileHandler.Handle(fileChange, recipient, outgoingQueue);
			}
		}

		private async Task HandleNewFile(SendFileHandler sendFileHandler, File file, ICollection<Recipient> recipients, System.Collections.Concurrent.BlockingCollection<ChunkPackage> outgoingQueue)
		{
			foreach (var recipient in recipients)
			{
				//todo try todo this in parallel
				await sendFileHandler.Handle(file, recipient, outgoingQueue);
			}
		}
	}
}