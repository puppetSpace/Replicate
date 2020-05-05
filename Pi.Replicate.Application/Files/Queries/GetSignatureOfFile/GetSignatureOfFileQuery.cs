using AutoMapper.Mappers;
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

namespace Pi.Replicate.Application.Files.Queries.GetSignatureOfFile
{
    public class GetSignatureOfFileQuery : IRequest<Result<ReadOnlyMemory<byte>>>
    {
		public Guid FileId { get; set; }
	}

	public class GetSignatureOfFileQueryHandler : IRequestHandler<GetSignatureOfFileQuery, Result<ReadOnlyMemory<byte>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT [Signature] FROM dbo.[File] WHERE Id = @FileId";

		public GetSignatureOfFileQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ReadOnlyMemory<byte>>> Handle(GetSignatureOfFileQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
					return Result<ReadOnlyMemory<byte>>.Success(await _database.QuerySingle<byte[]>(_selectStatement, new { request.FileId }));
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query {nameof(GetSignatureOfFileQuery)}");
				return Result<ReadOnlyMemory<byte>>.Failure();
			}
		}
	}
}
