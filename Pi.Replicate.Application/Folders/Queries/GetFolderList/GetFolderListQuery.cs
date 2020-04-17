using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderList
{
    public class GetFolderListQuery : IRequest<ICollection<string>>
    {

    }

    public class GetFolderListQueryHandler : IRequestHandler<GetFolderListQuery, ICollection<string>>
    {
        private readonly IDatabase _database;
        private readonly string _selectStatement = "SELECT Name from db.Folders";

        public GetFolderListQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<string>> Handle(GetFolderListQuery request, CancellationToken cancellationToken)
        {
            using(_database)
                return await _database.Query<string>(_selectStatement, null);
        }
    }
}
