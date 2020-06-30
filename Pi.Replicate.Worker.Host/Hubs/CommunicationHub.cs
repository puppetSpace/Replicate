using Grpc.Core;
using Observr;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public class CommunicationHub : Communicator.CommunicatorBase
	{
		private readonly IBroker _broker;

		public CommunicationHub(IBroker broker)
		{
			_broker = broker;
		}

		public override async Task<FolderWebhookResponse> FolderWebhookChanged(FolderWebhookRequest request, ServerCallContext context)
		{
			await _broker.Publish(new FolderWebhookChangedNotification { FolderId = Guid.Parse(request.FolderId), WebhookType = request.WebHookType, CallbackUrl = request.CallBackUrl });
			return new FolderWebhookResponse();
		}

		public override async Task<RecipientAddedResponse> RecipientAddedToFolder(RecipientAddedRequest request, ServerCallContext context)
		{
			await _broker.Publish(new RecipientsAddedToFolderNotification { FolderId = Guid.Parse(request.FolderId), Recipients = request.Recipients.Select(x => Guid.Parse(x)).ToList() });
			return new RecipientAddedResponse();
		}

		public override Task<ProbeResponse> ProbeRecipient(ProbeRequest request, ServerCallContext context)
		{
			WorkerLog.Instance.Information($"Someone is trying to verify this point");
			return Task.FromResult(new ProbeResponse { MachineName = Environment.MachineName });
		}
	}
}
