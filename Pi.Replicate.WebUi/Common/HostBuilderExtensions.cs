using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Shared;
using System;

namespace Pi.Replicate.WebUi.Common
{
	public static class HostBuilderExtensions
	{
		public static IHost SetWorkerBaseFolder(this IHost host)
		{
			var configuration = host.Services.GetService<IConfiguration>();
			var environmentValue = configuration.GetValue<string>("PIREPLICATOR_BASEPATH");
			if(!string.IsNullOrWhiteSpace(environmentValue))
			{
				PathBuilder.SetBasePath(environmentValue);
			}
			else
			{
				var proxy = host.Services.GetService<WorkerCommunicationProxy>();
				var basePath = proxy.GetBaseFolder().GetAwaiter().GetResult();
				if (string.IsNullOrWhiteSpace(basePath))
				{
					throw new InvalidOperationException("Basepath returned from worker is empty or null");
				}
				PathBuilder.SetBasePath(basePath);
			}

			return host;
		}
	}
}
