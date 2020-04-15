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
    public class GetFilesForFolderQuery : IRequest<ICollection<File>>
    {
        public GetFilesForFolderQuery(Guid folderId)
        {
            FolderId = folderId;
        }

        public Guid FolderId { get; }
    }

    public class GetFilesForFolderQueryHandler : IRequestHandler<GetFilesForFolderQuery, ICollection<File>>
    {
        private readonly IWorkerContext _workerContext;

        public GetFilesForFolderQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<ICollection<File>> Handle(GetFilesForFolderQuery request, CancellationToken cancellationToken)
        {
            return await _workerContext.FileRepository.GetForFolder(request.FolderId);
        }
    }
}
