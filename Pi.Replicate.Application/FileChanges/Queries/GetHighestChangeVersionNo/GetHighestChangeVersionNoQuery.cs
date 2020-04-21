using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChanges.Queries.GetHighestChangeVersionNo
{
    public class GetHighestChangeVersionNoQuery : IRequest<int>
    {
        public Guid FileId { get; set; }
    }

    public class GetHighestChangeVersionNoQueryHandler : IRequestHandler<GetHighestChangeVersionNoQuery, int>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "select coalesce(max(versionno),0) from dbo.FileChange where FileId = @FileId";

        public GetHighestChangeVersionNoQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<int> Handle(GetHighestChangeVersionNoQuery request, CancellationToken cancellationToken)
        {
            using (_database)
                return await _database.QuerySingle<int>(_selectStatement, new { request.FileId });
        }
    }
}
