﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pi.Replicate.Data;
using Pi.Replicate.Http;
using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.Notification;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Queueing;

namespace Pi.Replicate.Agent.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider Container { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureIoc(services);
            services.AddHostedService<WorkerHostedService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "Receive Agent API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Receive Agent API V1");
                c.RoutePrefix = string.Empty;
            });

            Container = app.ApplicationServices;
        }


        private void ConfigureIoc(IServiceCollection services)
        {
            services.AddSingleton<IWorkEventAggregator, WorkEventAggregator>();
            services.AddSingleton<IWorkItemQueueFactory, WorkItemQueueFactory>();
            services.AddSingleton<IWorkerFactory, WorkerFactory>();
            services.AddSingleton<WorkManager>();

            services.AddTransient<IRepository, Repository>();
            services.AddTransient<IUploadLink, HttpUploadLink>();
        }
    }
}
