using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl
{
    public class GetFoldersToCrawlQuery : IRequest<List<Folder>>
    {
        
    }

    public class GetFoldersToCrawlQueryHandler : IRequestHandler<GetFoldersToCrawlQuery, List<Folder>>
    {
        private readonly IWorkerContext _workerContext;

        public GetFoldersToCrawlQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public Task<List<Folder>> Handle(GetFoldersToCrawlQuery request, CancellationToken cancellationToken)
        {
            return _workerContext
                .Folders
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
