using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Queries.GetForFile
{
    public class GetForFileQuery : IRequest<ICollection<FileChunk>>
    {
        public Guid FileId { get; set; }

        public uint MinSequenceNo { get; set; } = 0;

        public uint MaxSequenceNo { get; set; } = int.MaxValue;
    }

    public class GetForFileQueryHandler : IRequestHandler<GetForFileQuery, ICollection<FileChunk>>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = "SELECT Id, FileId,SequenceNo,Value,ChunkSource WHERE FileId = @FileId and SequenceNo between @MinSequenceNo and @MaxSequenceNo";

        public GetForFileQueryHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<FileChunk>> Handle(GetForFileQuery request, CancellationToken cancellationToken)
        {
            using (_database)
                return await _database.Query<FileChunk>(_selectStatement, new { request.FileId, request.MinSequenceNo, request.MaxSequenceNo });

        }
    }
}
