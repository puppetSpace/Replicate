using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Queries.GetChunkPackages
{
    public class GetChunkPackagesQuery : IRequest<ICollection<ChunkPackage>>
    {
        
    }

    public class GetChunkPackagesQueryHandler : IRequestHandler<GetChunkPackagesQuery, ICollection<ChunkPackage>>
    {
        private readonly IWorkerContext _workerContext;

        public GetChunkPackagesQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }


        public async Task<ICollection<ChunkPackage>> Handle(GetChunkPackagesQuery request, CancellationToken cancellationToken)
        {
                return await _workerContext.ChunkPackageRepository.Get();
        }
    }
}
