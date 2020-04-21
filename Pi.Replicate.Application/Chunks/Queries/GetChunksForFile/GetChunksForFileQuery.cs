using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Queries.GetChunksForFile
{
    public class GetChunksForFileQuery : IRequest<ICollection<FileChunk>>
    {
        public Guid FileId { get; set; }

        public int MinSequenceNo { get; set; } = 0;

        public int MaxSequenceNo { get; set; } = int.MaxValue;
    }

    public class GetChunksForFileQueryHandler : IRequestHandler<GetChunksForFileQuery, ICollection<FileChunk>>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Id, FileId,SequenceNo,Value,ChunkSource FROM dbo.FileChunk WHERE FileId = @FileId and ChunkSource = 0 and SequenceNo between @MinSequenceNo and @MaxSequenceNo";

        public GetChunksForFileQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<FileChunk>> Handle(GetChunksForFileQuery request, CancellationToken cancellationToken)
        {
            using (_database)
                return await _database.Query<FileChunk>(_selectStatement, new { request.FileId, request.MinSequenceNo, request.MaxSequenceNo });

        }
    }
}
