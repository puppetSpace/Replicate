using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Queries.GetVerifiedRecipients
{
    public class GetVerifiedRecipientsQuery : IRequest<ICollection<Recipient>>
    {
        
    }

	public class GetVerifiedRecipientsQueryHandler : IRequestHandler<GetVerifiedRecipientsQuery, ICollection<Recipient>>
	{
		private readonly IDatabase _database;
		private const string _selectStatement = "SELECT Id,Name,Address FROM dbo.Recipient where Verified = 1";

		public GetVerifiedRecipientsQueryHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<ICollection<Recipient>> Handle(GetVerifiedRecipientsQuery request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				return await _database.Query<Recipient>(_selectStatement, null);
			}
		}
	}
}
