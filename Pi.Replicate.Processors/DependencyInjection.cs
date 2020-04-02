using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Processing.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    public static class DependencyInjection
    {
        public static void AddProcessing(this IServiceCollection services) 
        {
            services.AddTransient<FileCollectorFactory>();
        }
    }
}
