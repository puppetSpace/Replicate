using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFilesForFolder
{
    public class GetFilesForFolderQuery : IRequest<ICollection<File>>
    {
        public GetFilesForFolderQuery(Guid folderId)
        {
            FolderId = folderId;
        }

        public Guid FolderId { get; }
    }

    public class GetFilesForFolderQueryHandler : IRequestHandler<GetFilesForFolderQuery, ICollection<File>>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Id, FolderId,AmountOfChunks, Hash, LastModifiedDate, Name,Path, Signature,Size,Status FROM dbo.File WHERE FolderId = @FolderId";

        public GetFilesForFolderQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<File>> Handle(GetFilesForFolderQuery request, CancellationToken cancellationToken)
        {
            using(_database)
                return await _database.Query<File>(_selectStatement, new { request.FolderId });
        }
    }
}
