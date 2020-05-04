using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChunks.Queries.GetFailedTransmissions
{
    public class GetFailedFileChunkTransmissionsForRetryCommand : IRequest<ICollection<FailedFileChunk>>
    {
        
    }

	public class GetFailedFileChunkTransmissionsForRetryCommandHandler : IRequestHandler<GetFailedFileChunkTransmissionsForRetryCommand, ICollection<FailedFileChunk>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"SELECT fc.Id,fc.FileId,fc.SequenceNo, fc.Value
												  , re.Id, re.Name, re.Address
				FROM dbo.FileChunk fc
				INNER JOIN dbo.FailedTransmission ftn on ftn.FileChunkId = fc.Id
				LEFT JOIN dbo.Recipient re on re.Id = ftn.Recipient";
		private const string _deleteSatement = "DELETE FROM dbo.FileChunk WHERE Id in (SELECT FileChunkId from dbo.FailedTransmission);DELETE FROM dbo.FailedTransmission WHERE filechunkid is not null";

		public GetFailedFileChunkTransmissionsForRetryCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ICollection<FailedFileChunk>> Handle(GetFailedFileChunkTransmissionsForRetryCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var result = await _database.Query<FileChunk, Recipient, FailedFileChunk>(_selectStatement, null, (fc, re) => new FailedFileChunk { FileChunk = fc, Recipient = re });
				await _database.Execute(_deleteSatement, null);
				return result;
			}
		}
	}
}
