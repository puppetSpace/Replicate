using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Processing.Transmission
{
    public static class DependencyInjection
    {
        public static void AddTransmissionActions(this IServiceCollection services)
		{
			services.AddTransient<FileReceivedAction>();
			services.AddTransient<EofMessageReceivedAction>();
			services.AddTransient<TransmissionActionFactory>();
		}
    }
}
