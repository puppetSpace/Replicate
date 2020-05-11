using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetAvailableFolderNames
{
	public class GetAvailableFolderNamesQuery : IRequest<Result<ICollection<string>>>
	{

	}

	public class GetAvailableFolderNamessQueryHandler : IRequestHandler<GetAvailableFolderNamesQuery, Result<ICollection<string>>>
	{
		private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;
		private const string _selectStatement = "SELECT Name from dbo.Folder";

		public GetAvailableFolderNamessQueryHandler(IDatabase database, PathBuilder pathBuilder)
		{
			_database = database;
			_pathBuilder = pathBuilder;
		}

		public async Task<Result<ICollection<string>>> Handle(GetAvailableFolderNamesQuery request, CancellationToken cancellationToken)
		{
			var usedFolders = await _database.Query<string>(_selectStatement, null);

			var folderNames = System.IO.Directory.GetDirectories(_pathBuilder.BasePath)
				.Select(x => new System.IO.DirectoryInfo(x))
				.Where(x => usedFolders.All(y => !string.Equals(y, x.Name, StringComparison.OrdinalIgnoreCase)))
				.Select(x => x.Name)
				.ToList();

			return Result<ICollection<string>>.Success(folderNames);
		}
	}
}
