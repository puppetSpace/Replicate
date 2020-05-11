using Microsoft.AspNetCore.Components;
using Pi.Replicate.Application.Folders.Queries.GetFolderSettings;
using Pi.Replicate.Infrastructure.Services;
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
		public ProbeService ProbeService { get; set; }


		[Parameter]
		public FolderWebhookViewModel FolderWebhook { get; set; }

		protected ProbeResult ProbeResult { get; set; }

		
		protected async Task TestWebhook()
		{
			ProbeResult = await ProbeService.ProbePost(FolderWebhook.CallbackUrl, System.Text.Json.JsonSerializer.Deserialize<object>(FolderWebhook.WebHookTypeMessageStructure));
		}

	}
}
