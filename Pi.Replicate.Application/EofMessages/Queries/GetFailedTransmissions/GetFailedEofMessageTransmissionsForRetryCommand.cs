using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.EofMessages.Queries.GetFailedTransmissions
{
    public class GetFailedEofMessageTransmissionsForRetryCommand : IRequest<ICollection<FailedEofMessage>>
    {
        
    }

	public class GetFailedEofMessageTransmissionsForRetryCommandHandler : IRequestHandler<GetFailedEofMessageTransmissionsForRetryCommand, ICollection<FailedEofMessage>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = @"SELECT em.Id,em.FileId,em.AmountOfChunks, em.CreationTime
												  , re.Id, re.Name, re.Address
				FROM dbo.EofMessage em
				INNER JOIN dbo.FailedTransmission ftn on ftn.EofMessageId = em.Id
				LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId";
		private const string _deleteSatement = "DELETE FROM dbo.FailedTransmission WHERE EofMessageId is not null";

		public GetFailedEofMessageTransmissionsForRetryCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ICollection<FailedEofMessage>> Handle(GetFailedEofMessageTransmissionsForRetryCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var result = await _database.Query<EofMessage, Recipient, FailedEofMessage>(_selectStatement, null, (em, re) => new FailedEofMessage { EofMessage = em, Recipient = re });
				await _database.Execute(_deleteSatement, null);
				return result;
			}
		}
	}
}
