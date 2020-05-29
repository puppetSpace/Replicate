using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
    public static class DependecyInjection
    {
        public static void AddRepositories(this IServiceCollection services)
		{
			services.AddTransient<FolderRepository>();
			services.AddTransient<FileRepository>();
			services.AddTransient<TransmissionRepository>();
			services.AddTransient<RecipientRepository>();
			services.AddTransient<EofMessageRepository>();
			services.AddTransient<WebhookRepository>();
		}
    }
}
