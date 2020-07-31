using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Pi.Replicate.Worker.Host
{
	// todo check for double changes in incoming files
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
#endif
			.WriteTo.Console()
			.CreateLogger();


			CreateHostBuilder(args)
				.Build()
				.InitializeDatabase()
				.AddSystemSettingsFromDatabase()
				.SetWorkerBaseFolder()
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
