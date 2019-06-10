using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Agent.Send;
using Pi.Replicate.Data;
using Pi.Replicate.Http;
using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.Notification;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Queueing;
using Pi.Replicate.Schema;
using System;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent
{
    class Program
    {
        static async Task Main(string[] args)
        {
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Environment.CurrentDirectory)
				.AddJsonFile("appsettings.json")
				.Build();

			var services = new ServiceCollection();
			services.AddSingleton<IConfiguration>(configuration);
			services.AddSingleton<IWorkEventAggregator, WorkEventAggregator>();
			services.AddSingleton<IWorkItemQueueFactory, WorkItemQueueFactory>();
			services.AddSingleton<IWorkerFactory, WorkerFactory>();
			services.AddSingleton<WorkManager>();
			services.AddTransient<IRepository, Repository>();
			services.AddTransient<IUploadLink, HttpUploadLink>();
			services.AddTransient<Application>();

			services.AddDbContext<ReplicateDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("ReplicateStorage"), b => b.MigrationsAssembly("Pi.Replicate.Agent.Send")));

			var serviceProvider = services.BuildServiceProvider();
			//todo exception handling
			await Init(serviceProvider);

			await serviceProvider.GetService<Application>().Run();

			Console.ReadKey();
		}

		private static async Task Init(ServiceProvider serviceProvider)
		{
			//todo initialize ISettings with IConfiguration
			var repository = serviceProvider.GetService<IRepository>();
			await InitializeRootFolder(repository);
		}

		private static async Task InitializeRootFolder(IRepository repository)
		{
			var rootFolderSystemSettingValue = await repository.SystemSettingRepository.Get(SystemSettingsKeys.RootFolder);
			if (string.IsNullOrWhiteSpace(rootFolderSystemSettingValue?.Value)
				|| !System.IO.Directory.Exists(rootFolderSystemSettingValue.Value))
			{
				throw new InvalidOperationException(
					string.IsNullOrWhiteSpace(rootFolderSystemSettingValue?.Value) 
					? $"Systemsetting with key '{SystemSettingsKeys.RootFolder}' is empty or does not exists. Please add a systemsetting with these key that refers to an existsing directory you want shared" 
					: $"Systemsetting with key '{SystemSettingsKeys.RootFolder}' does not refer to an existing directory. CurrentValue: '{rootFolderSystemSettingValue.Value}'");
			}

			Shared.System.PathBuilder.Initialize(rootFolderSystemSettingValue.Value);
		}
	}
}
