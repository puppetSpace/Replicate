using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFilesForFolder
{
    public class GetFilesForFolderQuery : IRequest<List<File>>
    {
        public GetFilesForFolderQuery(Guid folderId)
        {
            FolderId = folderId;
        }

        public Guid FolderId { get; }
    }

    public class GetFilesForFolderQueryHandler : IRequestHandler<GetFilesForFolderQuery, List<File>>
    {
        private readonly IWorkerContext _workerContext;

        public GetFilesForFolderQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public Task<List<File>> Handle(GetFilesForFolderQuery request, CancellationToken cancellationToken)
        {
            return _workerContext.Files
                .Where(x => x.Folder.Id == request.FolderId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        }
    }
}
