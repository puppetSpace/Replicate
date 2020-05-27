using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Data.Db;
using System.Collections.Generic;
using Pi.Replicate.Domain;
using MediatR;
using Pi.Replicate.Application.SystemSettings.Queries.GetSystemSettingsForConfiguration;
using System;
using Microsoft.Extensions.Hosting;

namespace Pi.Replicate.Data
{
	public static class DependecyInjection
	{
		public static void AddData(this IServiceCollection services)
		{
			services.AddTransient<IDatabase, Database>();
			services.AddTransient<IDatabaseFactory, DatabaseFactory>();
		}

		public static IHost AddSystemSettingsFromDatabase(this IHost host)
		{
			var mediator = host.Services.GetService<IMediator>();
			var configuration = host.Services.GetService<IConfiguration>();
			var systemSettingResult = mediator.Send(new GetSystemSettingsForConfigurationQuery()).GetAwaiter().GetResult();
			if (systemSettingResult.WasSuccessful)
			{
				foreach (var systemSetting in systemSettingResult.Data)
					configuration[systemSetting.Key] = systemSetting.Value;
			}
			else
			{
				throw new InvalidOperationException("Unable to add systemsettings to configuration");
			}

			return host;
		}
	}
}
