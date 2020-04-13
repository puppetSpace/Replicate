using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.AddChunks
{
    public class AddChunkCommand : IRequest
    {
        public byte[] Chunk { get; set; }

        public int SequenceNo { get; set; }

        public File File { get; set; }

        public ChunkSource ChunkSource { get; set; }
    }

    public class AddChunkCommandHandler : IRequestHandler<AddChunkCommand>
    {
        private readonly IWorkerContext _workerContext;
        private readonly PathBuilder _pathBuilder;

        public AddChunkCommandHandler(IWorkerContext workerContext, PathBuilder pathBuilder)
        {
            _workerContext = workerContext;
            _pathBuilder = pathBuilder;
        }

        //transform each byte[] to a chunk and add it to the database.
        public async Task<Unit> Handle(AddChunkCommand request, CancellationToken cancellationToken)
        {
            var builtChunk = FileChunk.Build(request.File, request.SequenceNo, request.Chunk, ChunkSource.FromNewFile);
            _workerContext.FileChunks.Add(builtChunk);

            await _workerContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
