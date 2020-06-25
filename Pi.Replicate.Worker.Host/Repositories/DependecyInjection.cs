﻿using Microsoft.Extensions.DependencyInjection;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public static class DependecyInjection
	{
		public static void AddRepositories(this IServiceCollection services)
		{
			services.AddTransient<FolderRepository>();
			services.AddTransient<IFileRepository, FileRepository>();
			services.AddTransient<TransmissionRepository>();
			services.AddTransient<RecipientRepository>();
			services.AddTransient<IEofMessageRepository, EofMessageRepository>();
			services.AddTransient<WebhookRepository>();
			services.AddTransient<IFileChunkRepository, FileChunkRepository>();
			services.AddTransient<ConflictRepository>();
		}
	}
}
