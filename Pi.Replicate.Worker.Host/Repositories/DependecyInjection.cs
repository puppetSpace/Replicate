﻿using Microsoft.Extensions.DependencyInjection;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public static class DependecyInjection
	{
		public static void AddRepositories(this IServiceCollection services)
		{
			services.AddTransient<IFolderRepository,FolderRepository>();
			services.AddTransient<IFileRepository, FileRepository>();
			services.AddTransient<ITransmissionRepository,TransmissionRepository>();
			services.AddTransient<IRecipientRepository,RecipientRepository>();
			services.AddTransient<IEofMessageRepository, EofMessageRepository>();
			services.AddTransient<IWebhookRepository,WebhookRepository>();
			services.AddTransient<IFileChunkRepository, FileChunkRepository>();
			services.AddTransient<IConflictRepository,ConflictRepository>();
		}
	}
}
