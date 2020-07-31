using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Services;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.MSSqlServer.Sinks.MSSqlServer.Options;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host
{

	public static class WorkerStartupExtension
	{
		public static IHost InitializeWorker(this IHost host)
		{
			using (var database = host.Services.GetService<IDatabase>())
			{
				var webhookService = host.Services.GetService<IWebhookService>();
				var startup = new WorkerStartup(database, webhookService);
				startup.Initialize().GetAwaiter().GetResult();
			}
			return host;
		}

		public static IHost AddSystemSettingsFromDatabase(this IHost host)
		{
			var configuration = host.Services.GetService<IConfiguration>();
			using (var database = host.Services.GetService<IDatabase>())
			{
				var systemsettingsResult = database.Query<SystemSettingDto>("SELECT [Key],[Value] FROM dbo.SystemSetting", null).GetAwaiter().GetResult();
				if (systemsettingsResult.WasSuccessful)
				{
					foreach (var systemSetting in systemsettingsResult.Data)
						configuration[systemSetting.Key] = systemSetting.Value;
				}
				else
				{
					throw new InvalidOperationException("Unable to add systemsettings to configuration");
				}
			}

			return host;
		}

		public static IHost AttachLogSinks(this IHost host)
		{
			var telemetryProxy = host.Services.GetService<TelemetryProxy>();
			var configuration = host.Services.GetService<IConfiguration>();
			Log.Logger = new LoggerConfiguration()
				.WriteTo.Sink((ILogEventSink)Log.Logger)
				.WriteTo.MSSqlServer(configuration.GetConnectionString("ReplicateDatabase"), new SinkOptions { AutoCreateSqlTable = true, TableName = "log" })
				.WriteTo.Observers(events => events.Do(async evt =>
				 {
					 if (evt.Properties.ContainsKey("Context"))
						 await telemetryProxy.SendLog(evt);
				 }).Subscribe())
				.CreateLogger();
			return host;
		}

		public static IHost InitializeDatabase(this IHost host)
		{
#if !DEBUG
			var database = host.Services.GetService<IDatabase>();
			((IDatabaseInitializer)database).Initialize();
#endif
			return host;
		}

		public static IHost SetWorkerBaseFolder(this IHost host)
		{
			var configuration = host.Services.GetService<IConfiguration>();
			PathBuilder.SetBasePath(configuration.GetValue<string>(Shared.Constants.ReplicateBasePath));
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
				WorkerLog.Instance.Information("Initializing webhook service");
				await _webhookService.Initialize();
				using (_database)
				{
					WorkerLog.Instance.Information("Deleting unprocessed files from database");
					await _database.Execute("DELETE FROM dbo.FailedTransmission where FileId not in (select fileId from dbo.TransmissionResult)", null);
					await _database.Execute("DELETE FROM dbo.FailedTransmission where EofMessageId in (select Id from dbo.EofMessage where fileid not in (select fileId from dbo.TransmissionResult))",null);
					await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.TransmissionResult)", null);
					await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.EofMessage)", null);
					await _database.Execute("DELETE from dbo.EofMessage where fileid not in (select fileId from dbo.TransmissionResult)", null);
				}
			}
		}

		private class SystemSettingDto
		{
			public string Key { get; set; }

			public string Value { get; set; }
		}
	}
}
