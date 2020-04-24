using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolder
{
    public class GetFolderQuery : IRequest<Folder>
    {
        public Guid FolderId { get; set; }

    }

    public class GetFolderQueryHandler : IRequestHandler<GetFolderQuery, Folder>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Id, Name FROM dbo.Folder WHERE Id = @Id";

        public GetFolderQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Folder> Handle(GetFolderQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                return await _database.QuerySingle<Folder>(_selectStatement, new { Id = request.FolderId });
            }
        }
    }
}
