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

namespace Pi.Replicate.Application.Files.Queries.GetProcessedFilesForFolder
{
    public class GetProcessedFilesForFolderQuery : IRequest<List<File>>
    {
        public GetProcessedFilesForFolderQuery(Guid folderId)
        {
            FolderId = folderId;
        }

        public Guid FolderId { get; }
    }

    public class GetProcessedFilesForFolderQueryHandler : IRequestHandler<GetProcessedFilesForFolderQuery, List<File>>
    {
        private readonly IWorkerContext _workerContext;

        public GetProcessedFilesForFolderQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public Task<List<File>> Handle(GetProcessedFilesForFolderQuery request, CancellationToken cancellationToken)
        {
            return _workerContext.Files
                .Where(x => x.Folder.Id == request.FolderId && (x.Status == Domain.FileStatus.Processed || x.Status == FileStatus.New || x.Status == FileStatus.Changed))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        }
    }
}
