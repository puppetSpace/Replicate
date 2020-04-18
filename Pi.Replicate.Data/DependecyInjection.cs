using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Data.Db;
using System.Collections.Generic;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Data
{
    public static class DependecyInjection
    {
        public static void AddData(this IServiceCollection services, IConfiguration configuration, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            services.AddTransient<IDatabase, Database>();
            services.AddTransient<IDatabaseFactory, DatabaseFactory>();
        }


        public static void AddSystemSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var database = services.BuildServiceProvider().GetService<IDatabase>();
            var systemSettings = database.Query<SystemSetting>("SELECT [Key],[Value] from dbo.SystemSetting",null).GetAwaiter().GetResult();
            foreach (var systemSetting in systemSettings)
                configuration[systemSetting.Key] = systemSetting.Value;
        }
    }
}
