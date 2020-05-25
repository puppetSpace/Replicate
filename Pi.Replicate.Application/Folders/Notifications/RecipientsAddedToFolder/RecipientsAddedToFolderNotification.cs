using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Queries.GetFilesForFolder;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Notifications.RecipientsAddedToFolder
{
	public class RecipientsAddedToFolderNotification : INotification
	{
		public Guid FolderId { get; set; }

		public List<Recipient> Recipients { get; set; }
	}

	public class RecipientsAddedToFolderNotificationHandler : INotificationHandler<RecipientsAddedToFolderNotification>
	{
		private readonly IMediator _mediator;
		private readonly IMapper _mapper;
		private readonly WorkerQueueContainer _workerQueueContainer;

		public RecipientsAddedToFolderNotificationHandler(IMediator mediator, IMapper mapper, WorkerQueueContainer workerQueueContainer)
		{
			_mediator = mediator;
			_mapper = mapper;
			_workerQueueContainer = workerQueueContainer;
		}

		public async Task Handle(RecipientsAddedToFolderNotification notification, CancellationToken cancellationToken)
		{
			var fileResult = await _mediator.Send(new GetFilesForFolderQuery(notification.FolderId));
			if (fileResult.WasSuccessful)
			{
				foreach (var file in fileResult.Data)
				{
					var requestedFile = _mapper.Map<RequestFile>(file);
					requestedFile.Recipients = notification.Recipients;
					await _workerQueueContainer.ToSendFiles.Writer.WriteAsync(requestedFile, cancellationToken);
				}
			}
		}
	}
}
