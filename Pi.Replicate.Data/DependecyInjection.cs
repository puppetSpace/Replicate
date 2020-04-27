﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Data.Db;
using System.Collections.Generic;
using Pi.Replicate.Domain;
using MediatR;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettings;

namespace Pi.Replicate.Data
{
    public static class DependecyInjection
    {
        public static void AddData(this IServiceCollection services)
        {
            services.AddTransient<IDatabase, Database>();
            services.AddTransient<IDatabaseFactory, DatabaseFactory>();
        }


        public static void AddSystemSettingsFromDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var mediator = services.BuildServiceProvider().GetService<IMediator>();
            var systemSettings = mediator.Send(new GetSystemSettingsQuery()).GetAwaiter().GetResult();
            foreach (var systemSetting in systemSettings)
                configuration[systemSetting.Key] = systemSetting.Value;
        }
    }
}
