using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Queries.GetRecipientList
{
    public class GetRecipientListQuery : IRequest<RecipientListVm>
    {
        
    }

    public class GetRecipientsListQueryHandler : IRequestHandler<GetRecipientListQuery, RecipientListVm>
    {
        private readonly IWorkerContext _workerContext;

        public GetRecipientsListQueryHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<RecipientListVm> Handle(GetRecipientListQuery request, CancellationToken cancellationToken)
        {
            var recipients = await _workerContext.Recipients.ToListAsync();

            return new RecipientListVm { Recipients = recipients };
        }
    }
}
