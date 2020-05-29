using Microsoft.AspNetCore.Mvc;
using Observr;
using Pi.Replicate.Shared.Models;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class NotificationController : ControllerBase
	{
		private readonly IBroker _broker;

		public NotificationController(IBroker broker)
		{
			_broker = broker;
		}

		[HttpPost]
		public async Task<IActionResult> FolderWebhookChange([FromBody] FolderWebhookChangedNotification folderWebhookChangeNotification)
		{
			await _broker.Publish(folderWebhookChangeNotification);
			return Ok();
		}

		[HttpPost]
		public async Task<IActionResult> RecipientsAddedToFolder([FromBody] RecipientsAddedToFolderNotification recipientsAddedNotification)
		{
			await _broker.Publish(recipientsAddedNotification);
			return Ok();
		}
	}
}
