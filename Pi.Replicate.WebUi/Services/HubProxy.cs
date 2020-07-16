using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.WebUi.Services
{
    public class HubProxy
    {
		private readonly IConfiguration _configuration;

		public HubProxy(IConfiguration configuration)
		{
			_configuration = configuration;
		}
        
		public HubConnection BuildConnection(string hubName)
		{
			var workerApiUrl = GetHubHost();
			var hubConnection = new HubConnectionBuilder()
				.WithUrl($"{workerApiUrl}/{hubName}")
				.WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(30) })
				.Build();

			return hubConnection;
		}


		private string GetHubHost()
		{
			var workerApiUrl = _configuration[Constants.WorkerApiBaseAddressSetting];
			Log.Information($"Worker address: {workerApiUrl}");
			if (string.IsNullOrWhiteSpace(workerApiUrl))
				workerApiUrl = Environment.MachineName;

			return workerApiUrl;
		}

    }
}
