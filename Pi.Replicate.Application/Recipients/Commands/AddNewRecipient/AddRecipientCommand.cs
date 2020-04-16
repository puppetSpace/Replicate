using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Commands.AddRecipient
{
    public class AddRecipientCommand : IRequest<Recipient>
    {
        public string Name { get; set; }

        public string Address { get; set; }
    }

    public class AddRecipientCommandHandler : IRequestHandler<AddRecipientCommand, Recipient>
    {
        private readonly IWorkerContext _workerContext;

        public AddRecipientCommandHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<Recipient> Handle(AddRecipientCommand request, CancellationToken cancellationToken)
        {
                var builtRecipient = Recipient.Build(request.Name, request.Address);
                await _workerContext.RecipientRepository.Create(builtRecipient); ;

                return builtRecipient;
        }
    }
}
