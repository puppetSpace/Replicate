using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Behaviours;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Shared;
using System.Reflection;

namespace Pi.Replicate.Application
{
	public static class DependencyInjection
	{
		public static void AddApplication(this IServiceCollection services)
		{
			services.AddMediatR(Assembly.GetExecutingAssembly());
			services.AddAutoMapper(typeof(MappingProfile).Assembly);
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
			services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehavior<,>));
			services.AddTransient<WorkerCommunicationProxy>();
		}
	}
}
