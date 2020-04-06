using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application;
using Pi.Replicate.Processing;
using Pi.Replicate.Data;
using System;
using System.IO;

namespace Pi.Replicate.Worker.Console
{
    //todo on restart delete the new files in db. new files are not processed yet and chunks are not completely saved in db. must be processed first
	class Program
	{
		static async System.Threading.Tasks.Task Main(string[] args)
		{

            var serviceProvider = ConfigureServices().BuildServiceProvider();
            await serviceProvider.GetService<App>().Run();
        }

        static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            var config = LoadConfiguration();
            services.AddSingleton(config);

            services.AddApplication();
            services.AddData(config,ServiceLifetime.Transient);
            services.AddProcessing();


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
