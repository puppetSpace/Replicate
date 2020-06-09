using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Data;
using Serilog;

namespace Pi.Replicate.Worker.Host
{
	// todo check for double changes in incomming files
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
#if DEBUG
			.WriteTo.Debug()
			.WriteTo.Console()
#endif
			.CreateLogger();


			CreateHostBuilder(args)
				.Build()
				.AddSystemSettingsFromDatabase()
				.AttachLogSinks()
				.InitializeWorker()
				.Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				}).UseSerilog();
	}
}
