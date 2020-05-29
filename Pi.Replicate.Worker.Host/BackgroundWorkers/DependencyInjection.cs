using Microsoft.Extensions.DependencyInjection;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public static class DependencyInjection
	{
		public static void AddBackgroundServices(this IServiceCollection services)
		{
			services.AddHostedService<SystemOverviewWatcher>();
			services.AddHostedService<FolderWorker>();
			services.AddHostedService<FileExportWorker>();
			services.AddHostedService<FileDisassemblerWorker>();
			services.AddHostedService<FileChunkExportWorker>();
			services.AddHostedService<FileAssemblerWorker>();
			services.AddHostedService<RetryWorker>();
		}
	}
}
