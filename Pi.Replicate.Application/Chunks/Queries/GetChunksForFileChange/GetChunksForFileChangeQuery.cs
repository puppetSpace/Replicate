using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Queries.GetChunksForFileChange
{
    public class GetChunksForFileChangeQuery : IRequest<ICollection<FileChunk>>
    {
        public Guid FileId { get; set; }


        public int FileChangeVersionNo { get; set; }
    }

    public class GetChunksForFileChangeQueryHandler : IRequestHandler<GetChunksForFileChangeQuery, ICollection<FileChunk>>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Id, FileId,SequenceNo,Value,ChunkSource FROM dbo.FileChunk WHERE FileId = @FileId and ChunkSource = 1 and cast(SequenceNo as int) = @FileChangeVersionNo";

        public GetChunksForFileChangeQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<FileChunk>> Handle(GetChunksForFileChangeQuery request, CancellationToken cancellationToken)
        {
            using (_database)
                return await _database.Query<FileChunk>(_selectStatement, new { request.FileId, request.FileChangeVersionNo });

        }
    }
}
