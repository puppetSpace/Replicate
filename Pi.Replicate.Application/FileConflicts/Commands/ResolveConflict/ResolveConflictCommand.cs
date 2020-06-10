using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileConflicts.Commands.ResolveConflict
{
    public class ResolveConflictCommand : IRequest<Result>
    {
		public Guid ToDeleteConflictId { get; set; }

		public Guid ToDeleteFileId { get; set; }
	}

	public class ResolveConflictCommandHandler : IRequestHandler<ResolveConflictCommand, Result>
	{
		private readonly IDatabase _database;
		private const string _deleteStatementFileConflict = "DELETE FROM dbo.FileConflict WHERE Id = @Id";
		private const string _deleteStatementFile = "DELETE FROM dbo.[File] WHERE Id = @Id";

		public ResolveConflictCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> Handle(ResolveConflictCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				await _database.Execute(_deleteStatementFileConflict, new { Id = request.ToDeleteConflictId });
				await _database.Execute(_deleteStatementFile, new { Id = request.ToDeleteFileId });
				return Result.Success();
			}
		}
	}
}
