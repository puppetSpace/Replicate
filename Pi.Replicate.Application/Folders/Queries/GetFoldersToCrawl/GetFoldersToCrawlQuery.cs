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
    public class GetFoldersToCrawlQuery : IRequest<ICollection<Folder>>
    {
        
    }

    public class GetFoldersToCrawlQueryHandler : IRequestHandler<GetFoldersToCrawlQuery, ICollection<Folder>>
    {
        private readonly IWorkerContext _workerContext;

        public GetFoldersToCrawlQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<ICollection<Folder>> Handle(GetFoldersToCrawlQuery request, CancellationToken cancellationToken)
        {
            return await _workerContext
                .FolderRepository
                .Get();
        }
    }
}
