using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Observr;
using Pi.Replicate.Application;
using Pi.Replicate.Data;
using Pi.Replicate.Worker.Host.BackgroundWorkers;
using Polly;

namespace Pi.Replicate.Worker.Host
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddApplication();
			services.AddData();
			services.AddSystemSettingsFromDatabase(Configuration);
			services.AddObservr();
			services.AddHttpClient("default", client =>
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}).AddTransientHttpErrorPolicy(b => b.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5) }));

			services.AddControllers();

			//services.AddHostedService<FolderWorker>();
			//services.AddHostedService<FileExportWorker>();
			//services.AddHostedService<FileDisassemblerWorker>();
			//services.AddHostedService<ChunkExportWorker>();
			services.AddHostedService<FileAssemblerWorker>();
			//services.AddHostedService<RetryWorker>();

		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
