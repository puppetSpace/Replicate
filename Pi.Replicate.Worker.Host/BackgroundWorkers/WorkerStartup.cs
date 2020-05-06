using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Interfaces;
using Serilog;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host
{
	

	public static class WorkerStatupExtension
	{
		public static IHost CleanUp(this IHost host)
		{
			var database = (IDatabase)host.Services.GetService(typeof(IDatabase));
			var startup = new WorkerStartup(database);
			startup.Initialize().GetAwaiter().GetResult();
			return host;
		}

		private class WorkerStartup
		{
			private readonly IDatabase _database;

			public WorkerStartup(IDatabase database)
			{
				_database = database;
			}
			public async Task Initialize()
			{
				using (_database)
				{
					Log.Information("Deleting unprocessed files from database");
					await _database.Execute("DELETE FROM dbo.FailedTransmission where FileId not in (select fileId from dbo.TransmissionResult)", null);
					await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.TransmissionResult)", null);
					await _database.Execute("DELETE FROM dbo.[File] where Source = 0 and Id not in (select fileId from dbo.EofMessage)", null);
					await _database.Execute("DELETE from dbo.EofMessage where fileid not in (select fileId from dbo.TransmissionResult)", null);
				}
			}
		}
	}
}
