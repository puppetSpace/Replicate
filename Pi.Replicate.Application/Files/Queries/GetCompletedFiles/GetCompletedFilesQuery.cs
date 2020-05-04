using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetCompletedFiles
{
    public class GetCompletedFilesQuery : IRequest<ICollection<CompletedFileDto>>
    {
        
    }

	public class GetCompletedFilesQueryHandler : IRequestHandler<GetCompletedFilesQuery, ICollection<CompletedFileDto>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"
			select fi.Id, fi.FolderId,fi.[Name],fi.[Version], fi.Size,fi.LastModifiedDate, fi.[Path],fi.[Signature], fi.Source
			, eme.Id,eme.FileId,eme.AmountOfChunks,eme.CreationTime
			from dbo.[File] fi
			inner join dbo.EofMessage eme on eme.FileId = fi.Id
			left join dbo.FileChunk fc on fc.FileId = fi.Id
			where fi.Source = 1
			group by fi.Id, fi.FolderId,fi.[Name],fi.[Version], fi.Size,fi.LastModifiedDate, fi.[Path],fi.[Signature], fi.Source
			, eme.Id,eme.FileId,eme.AmountOfChunks,eme.CreationTime
			having sum(fc.SequenceNo) = (eme.AmountOfChunks*(eme.AmountOfChunks+1)) / 2";

		public GetCompletedFilesQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ICollection<CompletedFileDto>> Handle(GetCompletedFilesQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				return await _database.Query<File, EofMessage, CompletedFileDto>(_selectStatement, null
					, (f, e) => new CompletedFileDto { File = f, EofMessage = e });

			}
		}
	}
}
