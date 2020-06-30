using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Observr;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.BackgroundWorkers;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Hubs;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Processing.Transmission;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using Polly;
using System;
using System.Linq;
using System.Net.Http.Headers;

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
			services.AddTransient<IDatabase, Database>();
			services.AddTransient<IDatabaseFactory, DatabaseFactory>();
			services.AddSingleton<PathBuilder>();
			services.AddTransient<FileCollectorFactory>();
			services.AddSingleton<WorkerQueueContainer>();
			services.AddTransient<TelemetryProxy>();
			services.AddTransient<CommunicationProxy>();
			services.AddRepositories();
			services.AddWorkerServices();
			services.AddBackgroundServices();
			services.AddTransmissionActions();

			services.AddObservr();
			services.AddSignalR();
			services.AddGrpc();
			services.AddControllers();

			services.AddHttpClient("default", client =>
			{
				client.Timeout = TimeSpan.FromSeconds(5);
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			}).AddTransientHttpErrorPolicy(b => b.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1) }));

			services.AddHttpClient("webhook", client =>
			{
				client.DefaultRequestHeaders.Accept.Clear();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			});


			services.AddResponseCompression(opt =>
			{
				opt.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
			});
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
				endpoints.MapHub<SystemHub>("/systemHub");
				endpoints.MapHub<CommunicationHub>("/communicationHub");
				endpoints.MapGrpcService<NotificationHub>();
			});
		}
	}
}
