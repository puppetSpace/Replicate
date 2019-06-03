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
			services.AddSingleton<IWorkEventAggregator, WorkEventAggregator>();
			services.AddSingleton<IWorkItemQueueFactory, WorkItemQueueFactory>();
			services.AddSingleton<IWorkerFactory, WorkerFactory>();
			services.AddSingleton<WorkManager>();
			services.AddTransient<IRepository, Repository>();
			services.AddTransient<IUploadLink, HttpUploadLink>();
			services.AddTransient<Application>();

			services.AddDbContext<ReplicateDbContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("ReplicateStorage")));

			var serviceProvider = services.BuildServiceProvider();

			await serviceProvider.GetService<Application>().Run();
		}
    }
}
