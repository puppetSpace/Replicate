using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderName
{
    public class GetFolderNameQuery : IRequest<string>
    {
        public Guid FolderId { get; set; }

    }

    public class GetFolderQueryHandler : IRequestHandler<GetFolderNameQuery, string>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Name FROM dbo.Folders WHERE Id = @Id";

        public GetFolderQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<string> Handle(GetFolderNameQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                return await _database.QuerySingle<string>(_selectStatement, new { Id = request.FolderId });
            }
        }
    }
}
