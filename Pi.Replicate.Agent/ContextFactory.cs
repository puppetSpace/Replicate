using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent.Send
{
	public class ContextFactory : IDesignTimeDbContextFactory<ReplicateDbContext>
	{
		public ReplicateDbContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Environment.CurrentDirectory)
				.AddJsonFile("appsettings.json")
				.Build();

			var builder = new DbContextOptionsBuilder<ReplicateDbContext>()
				.UseSqlServer(configuration.GetConnectionString("ReplicateStorage"),b=>b.MigrationsAssembly("Pi.Replicate.Agent.Send"));

			return new ReplicateDbContext(builder.Options);

		}
	}
}
