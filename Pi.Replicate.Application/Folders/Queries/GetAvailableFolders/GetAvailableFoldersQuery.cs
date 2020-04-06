using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Queries.GetAvailableFolders
{
    public class GetAvailableFoldersQuery : IRequest<AvailableFoldersViewModel>
    {
        
    }

    public class GetAvailableFoldersQueryHandler : IRequestHandler<GetAvailableFoldersQuery, AvailableFoldersViewModel>
    {
        private readonly IWorkerContext _workerContext;
        private readonly PathBuilder _pathBuilder;

        public GetAvailableFoldersQueryHandler(IWorkerContext workerContext, PathBuilder pathBuilder)
        {
            _workerContext = workerContext;
            _pathBuilder = pathBuilder;
        }

        public async Task<AvailableFoldersViewModel> Handle(GetAvailableFoldersQuery request, CancellationToken cancellationToken)
        {
            var usedFolders = await _workerContext
                .Folders
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var availableFolders = System.IO.Directory.GetDirectories(_pathBuilder.BasePath)
                .Select(x => new System.IO.DirectoryInfo(x))
                .Where(x => usedFolders.All(y => !string.Equals(y.Name, x.Name, StringComparison.OrdinalIgnoreCase)))
                .Select(x => new AvailableFolderDto { Name = x.Name })
                .ToList();

            return new AvailableFoldersViewModel { Folders = availableFolders };

        }
    }
}
