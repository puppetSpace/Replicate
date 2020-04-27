using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipients
{
    public class GetRecipientsQuery : IRequest<ICollection<Recipient>>
    {
        
    }

    public class GetRecipientsQueryHandler : IRequestHandler<GetRecipientsQuery, ICollection<Recipient>>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Id,Name,Address,Verified FROM dbo.Recipient";

        public GetRecipientsQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<Recipient>> Handle(GetRecipientsQuery request, CancellationToken cancellationToken)
        {
            using(_database)
                return await _database.Query<Recipient>(_selectStatement, null);
        }
    }
}
