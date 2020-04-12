using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System.Linq;

namespace Pi.Replicate.Application.Files.Queries.GetFilesForProcessing
{
    public class GetFilesForProcessingQuery : IRequest<List<File>>
    {
        
    }

    public class GetFilesForProcessingQueryHandler : IRequestHandler<GetFilesForProcessingQuery, List<File>>
    {
        private readonly IWorkerContext _workerContext;

        public GetFilesForProcessingQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public Task<List<File>> Handle(GetFilesForProcessingQuery request, CancellationToken cancellationToken)
        {
            return _workerContext
            .Files
            .Where(x=>x.Status == FileStatus.New || x.Status == FileStatus.Changed)
            .AsNoTracking()
            .ToListAsync(); 
        }
    }
}