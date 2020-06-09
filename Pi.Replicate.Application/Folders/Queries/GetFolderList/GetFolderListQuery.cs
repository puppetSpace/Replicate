using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderList
{
    public class GetFolderListQuery : IRequest<Result<ICollection<FolderListItem>>>
    {

    }

    public class GetFolderListQueryHandler : IRequestHandler<GetFolderListQuery, Result<ICollection<FolderListItem>>>
    {
        private readonly IDatabase _database;
        private readonly string _selectStatement = @"SELECT fo.Id,fo.Name, iif(count(fc.Id) > 0,1,0) HasConflicts
			from dbo.Folder fo
			left join dbo.[File] fi on fi.FolderId = fo.Id
			left join dbo.FileConflict fc on fc.FileId = fi.Id
			group by fo.Id,fo.Name ";

        public GetFolderListQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Result<ICollection<FolderListItem>>> Handle(GetFolderListQuery request, CancellationToken cancellationToken)
        {
            using(_database)
				return Result<ICollection<FolderListItem>>.Success(await _database.Query<FolderListItem>(_selectStatement, null));
        }
    }
}
