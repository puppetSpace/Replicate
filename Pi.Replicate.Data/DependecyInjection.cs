using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Data.Db;

namespace Pi.Replicate.Data
{
    public static class DependecyInjection
    {
        public static void AddData(this IServiceCollection services, IConfiguration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddDbContext<WorkerContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ReplicateDatabase"));
            }, serviceLifetime);

            services.AddDbContext<SystemContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("ReplicateDatabase"));
            }, serviceLifetime);

            services.AddTransient<IWorkerContext>(provider => provider.GetService<WorkerContext>());
            services.AddTransient<ISystemContext>(provider => provider.GetService<SystemContext>());
        }


        public static void AddSystemSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var systemContext = services.BuildServiceProvider().GetService<ISystemContext>();
            foreach (var systemSetting in systemContext.SystemSettings)
                configuration[systemSetting.Key] = systemSetting.Value;
        }
    }
}
