using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.WebUi.Common;
using Serilog;
using System;

namespace Pi.Replicate.WebUi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json")
			.Build();

			Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.Enrich.FromLogContext()
			.WriteTo.Debug()
			.WriteTo.Console()
			.CreateLogger();

			CreateHostBuilder(args)
				.Build()
				.AddSystemSettingsFromDatabase()
				.SetWorkerBaseFolder()
				.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				})
			.UseSerilog();
	}

	public static class HostExtensions
	{
		public static IHost AddSystemSettingsFromDatabase(this IHost host)
		{
			var configuration = host.Services.GetService<IConfiguration>();
			using (var database = host.Services.GetService<IDatabase>())
			{
				var systemsettingsResult = database.Query<SystemSettingDto>("SELECT [Key],[Value] FROM dbo.SystemSetting", null).GetAwaiter().GetResult();
				foreach (var systemSetting in systemsettingsResult)
					configuration[systemSetting.Key] = systemSetting.Value;
			}

			return host;
		}

		private class SystemSettingDto
		{
			public string Key { get; set; }

			public string Value { get; set; }
		}
	}
}
