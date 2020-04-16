using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFailedFiles
{
    public class GetFailedFilesQuery : IRequest<ICollection<FailedFile>>
    {
        
    }

    public class GetFailedFilesQueryHandler : IRequestHandler<GetFailedFilesQuery, ICollection<FailedFile>>
    {
        private readonly IWorkerContext _workerContext;

        public GetFailedFilesQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<ICollection<FailedFile>> Handle(GetFailedFilesQuery request, CancellationToken cancellationToken)
        {
                var failedFiles = await _workerContext.FailedFileRepository.Get();
                await _workerContext.FailedFileRepository.DeleteAll();

                return failedFiles;
        }
    }
}
