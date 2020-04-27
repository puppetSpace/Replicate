using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application;
using Pi.Replicate.Data;
using Pi.Replicate.Workers;
using Polly;
using Serilog;
using System;
using System.IO;
using System.Net.Http.Headers;

namespace Pi.Replicate.Worker.Console
{
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
			services.AddData();
			services.AddSystemSettingsFromDatabase(config);
			services.AddHttpClient("default", client =>
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}).AddTransientHttpErrorPolicy(b => b.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) }));


			services.AddTransient<Startup>();
			services.AddTransient<FolderWorker>();
			services.AddTransient<FileProcessWorker>();
			services.AddTransient<FileExportWorker>();
			services.AddTransient<ChunkExportWorker>();


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
