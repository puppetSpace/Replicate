using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Commands.DeleteChunkPackage
{
    public class DeleteChunkPackageCommand : IRequest
    {
        public Guid ChunkPackageId { get; set; }
    }

    public class DeleteChunkPackageCommandHandler : IRequestHandler<DeleteChunkPackageCommand>
    {
        private readonly IWorkerContext _workerContext;

        public DeleteChunkPackageCommandHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<Unit> Handle(DeleteChunkPackageCommand request, CancellationToken cancellationToken)
        {
            await _workerContext.ChunkPackageRepository.Delete(request.ChunkPackageId);
            return Unit.Value;
        }
    }
}
