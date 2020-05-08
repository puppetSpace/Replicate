using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Infrastructure.Services;
using System;

namespace Pi.Replicate.Infrastructure
{
	public static class DependencyInjection
	{
		public static void AddInfrastructure(this IServiceCollection services)
		{
			services.AddTransient<IDeltaService,DeltaService>();
			services.AddTransient<ICompressionService,CompressionService>();
			services.AddTransient<ProbeService>();
		}
	}
}
