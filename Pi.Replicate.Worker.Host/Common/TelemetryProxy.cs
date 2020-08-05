using Microsoft.AspNetCore.SignalR;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Hubs;
using Serilog.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Common
{
	//maybe use grpc here to stream the data to the client
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

		private static readonly Dictionary<LogEventLevel, string> _shortLogNames = new Dictionary<LogEventLevel, string> {
			  { LogEventLevel.Debug, "DBG"}
			, { LogEventLevel.Error, "ERR"}
			, { LogEventLevel.Fatal, "FTL"}
			, { LogEventLevel.Information, "INF"}
			, { LogEventLevel.Verbose, "VER"}
			, { LogEventLevel.Warning, "WRN"}};
		internal async Task SendLog(LogEvent evt)
		{
			var logMessage = new LogMessage($"[{evt.Timestamp:HH:mm:ss} {_shortLogNames[evt.Level]}] {evt.RenderMessage()}", evt.Level);
			await _hubContext.Clients.All.SendAsync("ReceiveLog", logMessage);
		}

		//count not available on Channel yet
		//public async Task SendQueueCount(string queue,int count)
		//{
		//	await _hubContext.Clients.All.SendAsync("ReceieveQueueCount", queue,count);
		//}

	}
}
