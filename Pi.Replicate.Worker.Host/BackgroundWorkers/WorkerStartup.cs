using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Hubs;
using Serilog;
using Serilog.Core;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host
{
	
	public static class WorkerStartupExtension
	{
		public static IHost CleanUp(this IHost host)
		{
			var database = host.Services.GetService<IDatabase>();
			var webhookService = host.Services.GetService<IWebhookService>();
			var startup = new WorkerStartup(database, webhookService);
			startup.Initialize().GetAwaiter().GetResult();
			return host;
		}

		public static IHost AttachLogSinks(this IHost host)
		{
			var telemetryProxy = host.Services.GetService<TelemetryProxy>();
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Sink((ILogEventSink)Log.Logger)
				.WriteTo.Observers(events=> events.Do(async evt=> 
				{
					await telemetryProxy.SendLog(evt);
				}).Subscribe())
				.CreateLogger();
			return host;
		}



		private class WorkerStartup
		{
			private readonly IDatabase _database;
			private readonly IWebhookService _webhookService;

			public WorkerStartup(IDatabase database, IWebhookService webhookService)
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
