using MediatR;
using Observr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FolderWebhooks.Notifications.FolderWebhookChanged
{
    public class FolderWebhookChangedNotification : INotification
    {
		public Guid FolderId { get; set; }

		public string WebhookType { get; set; }

		public string CallbackUrl { get; set; }
	}

	public class FolderWebhookChangedNotificationHandler : INotificationHandler<FolderWebhookChangedNotification>
	{
		private readonly IBroker _broker;

		public FolderWebhookChangedNotificationHandler(IBroker broker)
		{
			_broker = broker;
		}

		public async  Task Handle(FolderWebhookChangedNotification notification, CancellationToken cancellationToken)
		{
			await _broker.Publish(notification);
		}
	}
}
