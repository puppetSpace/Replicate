using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Behaviours;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Shared;
using System.Reflection;

namespace Pi.Replicate.Application
{
    public static class DependencyInjection
    {
		public static void AddWebApplication(this IServiceCollection services)
		{
			services.AddMediatR(Assembly.GetExecutingAssembly());
			services.AddAutoMapper(typeof(MappingProfile).Assembly);
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
			services.AddTransient<WorkerNotificationProxy>();
		}

		public static void AddHostApplication(this IServiceCollection services)
		{
			services.AddSingleton<PathBuilder>();
			services.AddSingleton<WorkerQueueFactory>();
			services.AddSingleton<WebhookService>();
			services.AddTransient<FileCollectorFactory>();
			services.AddTransient<FileDisassemblerService>();
			services.AddTransient<TransmissionService>();
			services.AddTransient<FileService>();
			services.AddTransient<FileAssemblerServiceFactory>();
		}
	}
}
