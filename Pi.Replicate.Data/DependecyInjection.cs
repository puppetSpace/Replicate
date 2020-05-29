using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Data.Db;

namespace Pi.Replicate.Data
{
	public static class DependecyInjection
	{
		public static void AddData(this IServiceCollection services)
		{
			services.AddTransient<IDatabase, Database>();
			services.AddTransient<IDatabaseFactory, DatabaseFactory>();
		}
	}
}
