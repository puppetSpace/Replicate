using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Observr;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Behaviours;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Folders.Commands.AddNewFolder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application
{
    public static class DependencyInjection
    {
        public static void AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddSingleton<PathBuilder>();
            services.AddSingleton<WorkerQueueFactory>();
        }
    }
}
