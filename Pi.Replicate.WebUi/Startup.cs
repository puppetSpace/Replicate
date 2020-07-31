using System.Net.Http.Headers;
using Blazored.Toast;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Observr;
using Pi.Replicate.Application;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Data;
using Pi.Replicate.Shared;
using Pi.Replicate.WebUi.Services;
using Serilog;

namespace Pi.Replicate.WebUi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages()
                .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<IDatabase>()); ;
            services.AddServerSideBlazor();
            services.AddApplication();
            services.AddData();
            services.AddObservr();
			services.AddHttpClient();
			services.AddHttpClient("hostproxy",config=>
			{
				config.DefaultRequestHeaders.Accept.Clear();
				config.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			});
			services.AddTransient<WebhookTester>();
			services.AddTransient<OverviewService>();
			services.AddTransient<HubProxy>();
			services.AddBlazoredToast();
		}

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSerilogRequestLogging();
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
