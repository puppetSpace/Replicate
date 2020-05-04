using AutoMapper.Mappers;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetSignatureOfFile
{
    public class GetSignatureOfFileQuery : IRequest<ReadOnlyMemory<byte>>
    {
		public Guid FileId { get; set; }
	}

	public class GetSignatureOfFileQueryHandler : IRequestHandler<GetSignatureOfFileQuery, ReadOnlyMemory<byte>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT [Signature] FROM dbo.[File] WHERE Id = @FileId";

		public GetSignatureOfFileQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ReadOnlyMemory<byte>> Handle(GetSignatureOfFileQuery request, CancellationToken cancellationToken)
		{
			using (_database)
				return await _database.QuerySingle<byte[]>(_selectStatement, new { request.FileId });
		}
	}
}
