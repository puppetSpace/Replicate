using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages
{
	public class IndexBase : ComponentBase
	{
		private HubConnection _hubConnection;

		[Inject]
		public IConfiguration Configuration { get; set; }

		protected SystemOverview SystemOverview { get; set; } = new SystemOverview();

		protected override async Task OnInitializedAsync()
		{
			var workerApiUrl = Configuration["AppSettings:WorkerApiBaseAdress"];
			_hubConnection = new HubConnectionBuilder()
				.WithUrl($"{workerApiUrl}/systemHub")
				.Build();

			_hubConnection.On<SystemOverview>("ReceiveSystemOverview", (so) =>
			{
				SystemOverview = so;
				StateHasChanged();
			});

			await _hubConnection.StartAsync();
		}
	}
}
