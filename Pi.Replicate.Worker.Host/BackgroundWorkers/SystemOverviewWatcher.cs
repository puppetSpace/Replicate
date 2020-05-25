using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Hubs;
using Pi.Replicate.Worker.Host.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class SystemOverviewWatcher : BackgroundService
	{
		private readonly TelemetryProxy _telemetryProxy;

		public SystemOverviewWatcher(TelemetryProxy telemetryProxy)
		{
			_telemetryProxy = telemetryProxy;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.Run(async () =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					var systemservice = new SystemService();
					var overview = await systemservice.GetSystemOverview();
					await _telemetryProxy.SendSystemOverview(overview);
					await Task.Delay(2000);
				}
			});

		}
	}
}
