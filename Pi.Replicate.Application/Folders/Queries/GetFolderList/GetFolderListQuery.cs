using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderList
{
    public class GetFolderListQuery : IRequest<ICollection<FolderListItem>>
    {

    }

    public class GetFolderListQueryHandler : IRequestHandler<GetFolderListQuery, ICollection<FolderListItem>>
    {
        private readonly IDatabase _database;
        private readonly string _selectStatement = "SELECT Id,Name from dbo.Folder";

        public GetFolderListQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<FolderListItem>> Handle(GetFolderListQuery request, CancellationToken cancellationToken)
        {
            using(_database)
                return await _database.Query<FolderListItem>(_selectStatement, null);
        }
    }
}
