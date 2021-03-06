﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.WebUi.Common;
using Pi.Replicate.WebUi.Models;
using Pi.Replicate.WebUi.Services;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Pages
{
	public class IndexBase : ComponentBase,IDisposable
	{
		private HubConnection _hubConnection;

		[Inject]
		public IConfiguration Configuration { get; set; }

		[Inject]
		public OverviewService OverviewService { get; set; }

		[Inject]
		public HubProxy HubProxy { get; set; }

		protected SystemOverview SystemOverview { get; set; } = new SystemOverview();

		protected OverviewModel OverviewModel { get; set; } = new OverviewModel();

		protected RingList<LogMessage> LogItems { get; set; } = new RingList<LogMessage>(1000);

		protected override async Task OnInitializedAsync()
		{
			OverviewModel = await OverviewService.GetOverview();
			await ConnectToWorkerSignalrHub();
		}

		private async Task ConnectToWorkerSignalrHub()
		{
			try
			{
				_hubConnection = HubProxy.BuildConnection("systemHub");

				_hubConnection.On<SystemOverview>("ReceiveSystemOverview", (so) =>
				{
					SystemOverview = so;
					StateHasChanged();
				});

				_hubConnection.On<LogMessage>("ReceiveLog", (log) =>
				{
					LogItems.AddFirst(log);
					StateHasChanged();
				});

				await _hubConnection.StartAsync();

			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to connect to Signalr hub of Worker");
			}
		}

		protected static string FormLogMessageClass(LogEventLevel logEventLevel) => logEventLevel switch
		{
			LogEventLevel.Debug => $"site-overview-log-debug",
			LogEventLevel.Error => $"site-overview-log-error",
			LogEventLevel.Fatal => $"site-overview-log-fatal",
			LogEventLevel.Information => $"site-overview-log-information",
			LogEventLevel.Verbose => $"site-overview-log-verbose",
			LogEventLevel.Warning => $"site-overview-log-warning",
			_ => "site-overview-log-information"
		};

		public void Dispose()
		{
			_hubConnection?.DisposeAsync();
		}
	}
}
