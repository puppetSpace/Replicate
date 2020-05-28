using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Observr;
using Pi.Replicate.WebUi.Models;
using Pi.Replicate.WebUi.Pages.Settings.Components;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages.Settings
{
	//todo notify user of unsaved setting when navigating away
	public class SettingsOverviewBase : ComponentBase
	{

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Inject]
		public IToastService ToastService { get; set; }

		protected General GeneralSettings { get; set; }

		protected Recipients RecipientSettings { get; set; }


		public async Task NotifySaveClicked()
		{
			if (await GeneralSettings.Save() && await RecipientSettings.Save())
			{
				await JSRuntime.InvokeVoidAsync("scrollIntoView", "settings-top");
				ToastService.ShowInfo("Settings saved");
			}

		}

	}
}
