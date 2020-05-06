using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.MarkFileAsFailed
{
    public class MarkFileAsFailedCommand : IRequest<Result>
    {
		public Guid FileId { get; set; }
	}

	public class MarkFileAsFailedCommandHandler : IRequestHandler<MarkFileAsFailedCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _updateStatement = @"UPDATE dbo.[File] SET [Status] = 1 WHERE Id = @Id";

		public MarkFileAsFailedCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(MarkFileAsFailedCommand request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
					await _database.Execute(_updateStatement, new { Id = request.FileId });

				return Result.Success();
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing command '{nameof(MarkFileAsFailedCommand)}'");
				return Result.Failure();
			}
		}
	}
}
