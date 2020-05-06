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

namespace Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedEofMessageTransmissions
{
    public class GetFailedEofMessageTransmissionsForRetryQuery : IRequest<Result<ICollection<FailedEofMessage>>>
    {
        
    }

	public class GetFailedEofMessageTransmissionsForRetryQueryHandler : IRequestHandler<GetFailedEofMessageTransmissionsForRetryQuery, Result<ICollection<FailedEofMessage>>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"SELECT em.Id,em.FileId,em.AmountOfChunks, em.CreationTime
												  , re.Id, re.Name, re.Address
				FROM dbo.EofMessage em
				INNER JOIN dbo.FailedTransmission ftn on ftn.EofMessageId = em.Id
				LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId";

		public GetFailedEofMessageTransmissionsForRetryQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<FailedEofMessage>>> Handle(GetFailedEofMessageTransmissionsForRetryQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
				{
					var result = await _database.Query<EofMessage, Recipient, FailedEofMessage>(_selectStatement, null, (em, re) => new FailedEofMessage { EofMessage = em, Recipient = re });
					return Result<ICollection<FailedEofMessage>>.Success(result);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetFailedEofMessageTransmissionsForRetryQuery)}'");
				return Result<ICollection<FailedEofMessage>>.Failure();
			}
		}
	}
}
