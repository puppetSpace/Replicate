using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.AddChunks
{
    public class AddChunksCommand : IRequest
    {
        public List<byte> Chunks { get; set; }

        public byte[] Hash { get; set; }

        public File File { get; set; }

        public ChunkSource ChunkSource { get; set; }
    }

    public class AddChunksCommandHandler : IRequestHandler<AddChunksCommand>
    {
        private readonly IWorkerContext _workerContext;

        public AddChunksCommandHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public Task<Unit> Handle(AddChunksCommand request, CancellationToken cancellationToken)
        {
            foreach(var chunk in request.Chunks)
            {

            }
        }
    }
}
