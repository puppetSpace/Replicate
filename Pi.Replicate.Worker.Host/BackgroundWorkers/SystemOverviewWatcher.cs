using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Worker.Host.Hubs;
using Pi.Replicate.Worker.Host.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class SystemOverviewWatcher : BackgroundService
	{
		private readonly IHubContext<SystemHub> _hubContext;

		public SystemOverviewWatcher(IHubContext<SystemHub> hubContext)
		{
			_hubContext = hubContext;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			return Task.Run(async () =>
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					var systemservice = new SystemService();
					var overview = await systemservice.GetSystemOverview();
					await _hubContext.Clients.All.SendAsync("ReceiveSystemOverview", overview);
					await Task.Delay(2000);
				}
			});

		}
	}
}
