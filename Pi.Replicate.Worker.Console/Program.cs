using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application;
using Pi.Replicate.Data;
using Pi.Replicate.Workers;
using Serilog;
using System.IO;

namespace Pi.Replicate.Worker.Console
{
	//todo on restart delete the new files in db. new files are not processed yet and chunks are not completely saved in db. must be processed first
	class Program
	{
		static async System.Threading.Tasks.Task Main(string[] args)
		{
			var config = LoadConfiguration();

			Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(config)
			.Enrich.FromLogContext()
			.WriteTo.Debug()
			.WriteTo.Console()
			.CreateLogger();

			var serviceProvider = ConfigureServices(config).BuildServiceProvider();
			await serviceProvider.GetService<App>().Run();
		}

		static IServiceCollection ConfigureServices(IConfiguration config)
		{
			IServiceCollection services = new ServiceCollection();

			services.AddSingleton(config);

			services.AddApplication();
			services.AddData(config, ServiceLifetime.Transient);
			services.AddSystemSettings(config);
			services.AddHttpClient();


			services.AddTransient<FolderWorker>();
			services.AddTransient<FileProcessForExportWorker>();


			// required to run the application
			services.AddTransient<App>();

			return services;
		}

		static IConfiguration LoadConfiguration()
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

			return builder.Build();
		}
	}
}
