using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetAvailableFolders
{
	public class GetAvailableFoldersQuery : IRequest<ICollection<string>>
	{

	}

	public class GetAvailableFoldersQueryHandler : IRequestHandler<GetAvailableFoldersQuery, ICollection<string>>
	{
		private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;
		private const string _selectStatement = "SELECT Name from dbo.Folder";

		public GetAvailableFoldersQueryHandler(IDatabase database, PathBuilder pathBuilder)
		{
			_database = database;
			_pathBuilder = pathBuilder;
		}

		public async Task<ICollection<string>> Handle(GetAvailableFoldersQuery request, CancellationToken cancellationToken)
		{
			var usedFolders = await _database.Query<string>(_selectStatement, null);

			return System.IO.Directory.GetDirectories(_pathBuilder.BasePath)
				.Select(x => new System.IO.DirectoryInfo(x))
				.Where(x => usedFolders.All(y => !string.Equals(y, x.Name, StringComparison.OrdinalIgnoreCase)))
				.Select(x => x.Name)
				.ToList();
		}
	}
}
