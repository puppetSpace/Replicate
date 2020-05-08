using MediatR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Behaviours
{
	public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TResponse : Result
	{
		public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
		{
			try
			{
				var response = await next();
				return response;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing '{typeof(TRequest).Name}'");

				if(typeof(TResponse).IsGenericType)
				{
					var typeArguments = typeof(TResponse).GetGenericArguments();
					return Activator.CreateInstance(typeof(Result<>).MakeGenericType(typeArguments)) as TResponse;
				}
				else
				{
					return Result.Failure() as TResponse;
				}
			}
		}
	}
}
