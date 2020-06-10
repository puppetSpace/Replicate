using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileConflicts.Queries.GetFileConflicts
{
    public class GetFileConflictsQuery : IRequest<Result<ICollection<FileConflictModel>>>
    {
		public Guid FolderId { get; set; }

	}

	public class GetConflictQueryHandler : IRequestHandler<GetFileConflictsQuery, Result<ICollection<FileConflictModel>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"select fc.Id, fc.[Type]
			,fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name RecipientName
			from dbo.FileConflict fc
			left join dbo.[File] fi on fi.Id = fc.FileId
			left join dbo.TransmissionResult tr on tr.FileId = fi.Id
			left join dbo.Recipient re on re.Id = tr.RecipientId
			where fi.FolderId = @FolderId
			group by fc.Id, fc.[Type],fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name";
		private const string _selectStatementConflictSameVersion = @"select fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name RecipientName
			from dbo.[File] fi
			left join dbo.TransmissionResult tr on tr.FileId = fi.Id
			left join dbo.Recipient re on re.Id = tr.RecipientId
			where fi.Id <> @FileId
			and fi.Path = @Path
			and fi.Version = @Version
			group by fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name";
		private const string _selectStatementConflictHigherVersion = @"select fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name RecipientName
			from dbo.[File] fi
			left join dbo.TransmissionResult tr on tr.FileId = fi.Id
			left join dbo.Recipient re on re.Id = tr.RecipientId
			where fi.Id <> @FileId
			and fi.Path = @Path
			and fi.Version > @Version
			group by fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name";
		private const string _selectStatementConflictMissingVersion = @"select fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name RecipientName
			from dbo.[File] fi
			left join dbo.TransmissionResult tr on tr.FileId = fi.Id
			left join dbo.Recipient re on re.Id = tr.RecipientId
			where fi.Id <> @FileId
			and fi.Path = @Path
			and fi.Version <> @Version
			group by fi.Id,fi.[Name],fi.[Version],fi.Size,fi.LastModifiedDate,fi.[Path],re.Name";

		public GetConflictQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<FileConflictModel>>> Handle(GetFileConflictsQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var data = await _database.Query<FileConflictModel, FileModel, FileConflictModel>(_selectStatement, new { request.FolderId }, (fc, fi) =>
				{
					fc.File = fi;
					return fc;
				});

				await EnrichWithConflictingFiles(data);

				return Result<ICollection<FileConflictModel>>.Success(data);
			}
		}

		private async Task EnrichWithConflictingFiles(ICollection<FileConflictModel> data)
		{
			foreach (var conflict in data)
			{
				if (conflict.Type == FileConflictType.SameVersion)
					conflict.ConflictingFiles = await _database.Query<FileModel>(_selectStatementConflictSameVersion, new { FileId = conflict.File.Id, Path = conflict.File.Path, Version = conflict.File.Version });
				else if(conflict.Type == FileConflictType.HigherVersion)
					conflict.ConflictingFiles = await _database.Query<FileModel>(_selectStatementConflictHigherVersion, new { FileId = conflict.File.Id, Path = conflict.File.Path, Version = conflict.File.Version });
				else if(conflict.Type == FileConflictType.MissingVersion)
					conflict.ConflictingFiles = await _database.Query<FileModel>(_selectStatementConflictMissingVersion, new { FileId = conflict.File.Id, Path = conflict.File.Path, Version = conflict.File.Version });
			}
		}
	}
}
