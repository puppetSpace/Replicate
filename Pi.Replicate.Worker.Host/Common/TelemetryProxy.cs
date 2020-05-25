using Microsoft.AspNetCore.SignalR;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Common
{
    public class TelemetryProxy
    {
		private readonly IHubContext<SystemHub> _hubContext;

		public TelemetryProxy(IHubContext<SystemHub> hubContext)
		{
			_hubContext = hubContext;
		}

		public async Task SendSystemOverview(SystemOverview systemOverview)
		{
			await _hubContext.Clients.All.SendAsync("ReceiveSystemOverview", systemOverview);
		}

		//count not available on Channel yet
		//public async Task SendQueueCount(string queue,int count)
		//{
		//	await _hubContext.Clients.All.SendAsync("ReceieveQueueCount", queue,count);
		//}

	}
}
