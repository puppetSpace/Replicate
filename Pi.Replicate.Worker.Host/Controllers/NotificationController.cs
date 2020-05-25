using MediatR;
using Microsoft.AspNetCore.Mvc;
using Observr;
using Pi.Replicate.Application.FolderWebhooks.Notifications.FolderWebhookChanged;
using Pi.Replicate.Application.Folders.Notifications.RecipientsAddedToFolder;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		private readonly IMediator _mediator;

		public NotificationController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpPost]
		public async Task<IActionResult> FolderWebhookChange([FromBody] FolderWebhookChangedNotification folderWebhookChangeNotification)
		{
			await _mediator.Publish(folderWebhookChangeNotification);
			return Ok();
		}

		[HttpPost]
		public async Task<IActionResult> RecipientsAddedToFolder([FromBody] RecipientsAddedToFolderNotification recipientsAddedNotification)
		{
			await _mediator.Publish(recipientsAddedNotification);
			return Ok();
		}
	}
}
