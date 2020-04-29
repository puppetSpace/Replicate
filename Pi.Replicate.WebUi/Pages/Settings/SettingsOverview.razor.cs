using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Observr;
using Pi.Replicate.WebUi.Models;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings
{
	public class SettingsOverviewBase : ComponentBase
	{
		[Inject]
		protected IBroker SaveNotifier { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }


		public async Task NotifySaveClicked()
		{
			await SaveNotifier.Publish(new SettingsSaveNotificationMessage());
			await JSRuntime.InvokeVoidAsync("scrollIntoView", "settings-top");
		}

	}
}
