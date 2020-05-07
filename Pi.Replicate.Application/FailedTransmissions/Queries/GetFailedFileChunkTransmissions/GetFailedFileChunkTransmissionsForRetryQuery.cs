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

namespace Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileChunkTransmissions
{
    public class GetFailedFileChunkTransmissionsForRetryQuery : IRequest<Result<ICollection<FailedFileChunk>>>
    {
        
    }

	public class GetFailedFileChunkTransmissionsForRetryQueryHandler : IRequestHandler<GetFailedFileChunkTransmissionsForRetryQuery, Result<ICollection<FailedFileChunk>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"SELECT fc.Id,fc.FileId,fc.SequenceNo, fc.Value
												  , re.Id, re.Name, re.Address
				FROM dbo.FileChunk fc
				INNER JOIN dbo.FailedTransmission ftn on ftn.FileChunkId = fc.Id
				LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId";

		public GetFailedFileChunkTransmissionsForRetryQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<FailedFileChunk>>> Handle(GetFailedFileChunkTransmissionsForRetryQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
				{
					var result = await _database.Query<FileChunk, Recipient, FailedFileChunk>(_selectStatement, null, (fc, re) => new FailedFileChunk { FileChunk = fc, Recipient = re });
					return Result<ICollection<FailedFileChunk>>.Success(result);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetFailedFileChunkTransmissionsForRetryQuery)}'");
				return Result<ICollection<FailedFileChunk>>.Failure();
			}
		}
	}
}
