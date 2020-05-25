using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private readonly IDatabase _database;
		private readonly WorkerQueueContainer _workerQueueContainer;

		public RecipientsAddedToFolderNotificationHandler(IDatabase database, WorkerQueueContainer workerQueueContainer)
		{
			_database = database;
			_workerQueueContainer = workerQueueContainer;
		}

		public Task Handle(RecipientsAddedToFolderNotification notification, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
