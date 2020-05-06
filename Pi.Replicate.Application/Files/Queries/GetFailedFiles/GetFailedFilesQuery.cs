using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFailedFiles
{
    public class GetFailedFilesQuery : IRequest<Result<ICollection<File>>>
    {
        
    }

	public class GetFailedFileQueryHandler : IRequestHandler<GetFailedFilesQuery, Result<ICollection<File>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT Id,FolderId,Name,Version,Size,LastModifiedDate,Path,Source FROM dbo.[File] WHERE [Status] = 1";

		public GetFailedFileQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<File>>> Handle(GetFailedFilesQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
					return Result<ICollection<File>>.Success(await _database.Query<File>(_selectStatement, null));
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetFailedFilesQuery)}'");
				return Result<ICollection<File>>.Failure();
			}
		}
	}
}
