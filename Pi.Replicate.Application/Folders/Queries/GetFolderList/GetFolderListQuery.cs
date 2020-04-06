using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using AutoMapper;

namespace Pi.Replicate.Application.Folders.Queries.GetFolderList
{
    public class GetFolderListQuery :IRequest<FolderListViewModel>
    {
        
    }

    public class GetFolderListQueryHandler : IRequestHandler<GetFolderListQuery, FolderListViewModel>
    {
        private readonly IWorkerContext _workerContext;
        private readonly IMapper _mapper;

        public GetFolderListQueryHandler(IWorkerContext workerContext, IMapper mapper)
        {
            _workerContext = workerContext;
            _mapper = mapper;
        }

        public async Task<FolderListViewModel> Handle(GetFolderListQuery request, CancellationToken cancellationToken)
        {
            var folders = await _workerContext
                .Folders
                .ProjectTo<FolderLookupDto>(_mapper.ConfigurationProvider)
                .OrderBy(x=>x.Name)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return new FolderListViewModel { Folders = folders };
        }
    }
}
