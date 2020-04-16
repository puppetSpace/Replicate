using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder
{
    public class GetRecipientsForFolderQuery : IRequest<ICollection<Recipient>>
    {
        public Guid FolderId { get; set; }
    }

    public class GetRecipientsForFolderQueryHandler : IRequestHandler<GetRecipientsForFolderQuery, ICollection<Recipient>>
    {
        private readonly IWorkerContext _workerContext;

        public GetRecipientsForFolderQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }
        public async Task<ICollection<Recipient>> Handle(GetRecipientsForFolderQuery request, CancellationToken cancellationToken)
        {
                return await _workerContext.RecipientRepository.GetRecipientsForFolder(request.FolderId);
        }
    }
}