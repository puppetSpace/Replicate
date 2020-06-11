using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			services.AddSingleton<IFileConflictService,FileConflictService>();
		}
    }
}
