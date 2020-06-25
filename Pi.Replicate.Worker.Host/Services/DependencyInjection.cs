using Microsoft.Extensions.DependencyInjection;

namespace Pi.Replicate.Worker.Host.Services
{
	public static class DependencyInjection
	{
		public static void AddWorkerServices(this IServiceCollection services)
		{
			services.AddTransient<FileService>();
			services.AddTransient<IDeltaService, DeltaService>();
			services.AddTransient<ICompressionService, CompressionService>();
			services.AddTransient<FileDisassemblerService>();
			services.AddTransient<FileAssemblerServiceFactory>();
			services.AddTransient<FileChunkService>();
			services.AddTransient<TransmissionService>();
			services.AddSingleton<IWebhookService, WebhookService>();
			services.AddSingleton<IFileConflictService, FileConflictService>();
		}
	}
}
