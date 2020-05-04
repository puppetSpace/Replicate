using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetPreviousSignatureOfFile
{
    public class GetPreviousSignatureOfFileQuery : IRequest<ReadOnlyMemory<byte>>
    {
		public Guid FileId { get; set; }
	}

	public class GetPreviousSignatureOfFileQueryHandler : IRequestHandler<GetPreviousSignatureOfFileQuery, ReadOnlyMemory<byte>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"
			WITH file_cte([path],[version])
			AS (SELECT [path],[version] FROM dbo.[File] WHERE Id = @FileId)
			SELECT [signature] 
			FROM dbo.[File] fi
			INENR JOIN file_cte fic ON fic.[path] = fi.[Path] AND fi.[Version] = fic.[version] - 1 ";

		public GetPreviousSignatureOfFileQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ReadOnlyMemory<byte>> Handle(GetPreviousSignatureOfFileQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				return await _database.QuerySingle<byte[]>(_selectStatement, new { request.FileId });
			}
		}
	}
}
