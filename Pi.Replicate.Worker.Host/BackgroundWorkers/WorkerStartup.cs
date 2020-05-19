using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Serilog;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host
{
	

	public static class WorkerStartupExtension
	{
		public static IHost CleanUp(this IHost host)
		{
			var database = (IDatabase)host.Services.GetService(typeof(IDatabase));
			var webhookService = (WebhookService)host.Services.GetService(typeof(WebhookService));
			var startup = new WorkerStartup(database, webhookService);
			startup.Initialize().GetAwaiter().GetResult();
			return host;
		}

		private class WorkerStartup
		{
			private readonly IDatabase _database;
			private readonly WebhookService _webhookService;

			public WorkerStartup(IDatabase database, WebhookService webhookService)
			{
				_database = database;
				_webhookService = webhookService;
			}
			public async Task Initialize()
			{
				Log.Information("Initializing webhook service");
				await _webhookService.Initialize();
				using (_database)
				{
					Log.Information("Deleting unprocessed files from database");
					await _database.Execute("DELETE FROM dbo.FailedTransmission where FileId not in (select fileId from dbo.TransmissionResult)", null);
					await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.TransmissionResult)", null);
					await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.EofMessage)", null);
					await _database.Execute("DELETE from dbo.EofMessage where fileid not in (select fileId from dbo.TransmissionResult)", null);
				}
			}
		}
	}
}
