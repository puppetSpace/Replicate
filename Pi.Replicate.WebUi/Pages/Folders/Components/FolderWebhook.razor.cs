using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Queries.GetFolderSettings;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Folders.Components
{
    public class FolderWebhookBase : ComponentBase
    {

		[Inject]
		public WebhookTester WebhookTester { get; set; }


		[Parameter]
		public FolderWebhookViewModel FolderWebhook { get; set; }

		protected WebhookResult WebhookResult { get; set; }

		
		protected async Task TestWebhook()
		{
			WebhookResult = await WebhookTester.Test(FolderWebhook.CallbackUrl, System.Text.Json.JsonSerializer.Deserialize<object>(FolderWebhook.WebHookTypeMessageStructure));
		}

	}
}
