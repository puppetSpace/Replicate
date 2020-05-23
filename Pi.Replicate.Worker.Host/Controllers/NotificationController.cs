using Microsoft.AspNetCore.Mvc;
using Observr;
using Pi.Replicate.Application.Notifications.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public async Task<IActionResult> FolderWebhookChange([FromBody] FolderWebhookChangeNotification folderWebhookChangeNotification)
		{
			await _broker.Publish(folderWebhookChangeNotification);
			return Ok();
		}

		[HttpPost]
		public async Task<IActionResult> RecipientsAdded([FromBody] RecipientsAddedNotification recipientsAddedNotification)
		{
			//await _broker.Publish(folderWebhookChangeNotification);
			return Ok();
		}
	}
}
