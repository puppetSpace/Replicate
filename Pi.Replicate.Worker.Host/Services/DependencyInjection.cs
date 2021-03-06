﻿using Microsoft.Extensions.DependencyInjection;

namespace Pi.Replicate.Worker.Host.Services
{
	public static class DependencyInjection
	{
		public static void AddWorkerServices(this IServiceCollection services)
		{
			services.AddTransient<FileService>();
			services.AddTransient<FileDisassemblerServiceFactory>();
			services.AddTransient<FileAssemblerServiceFactory>();
			//services.AddTransient<ITransmissionLink, RestTransmissionLink>();
			services.AddTransient<ITransmissionLink, GrpcTransmissionLink>();
			services.AddTransient<TransmissionService>();
			services.AddSingleton<IWebhookService, WebhookService>();
			services.AddSingleton<IFileConflictService, FileConflictService>();
		}
	}
}
