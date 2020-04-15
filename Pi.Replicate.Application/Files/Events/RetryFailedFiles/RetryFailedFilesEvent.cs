using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Events.SendFileToRecipient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Events.RetryFailedFiles
{
    public class RetryFailedFilesEvent : IRequest
    {
        
    }

    public class RetryFailedFilesEventHandler : IRequestHandler<RetryFailedFilesEvent>
    {
        private readonly IWorkerContext _workerContext;
        private readonly IMediator _mediator;

        public RetryFailedFilesEventHandler(IWorkerContext workerContext, IMediator mediator)
        {
            _workerContext = workerContext;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(RetryFailedFilesEvent request, CancellationToken cancellationToken)
        {
            var failedFiles = await _workerContext.FailedFileRepository.Get();
            await _workerContext.FailedFileRepository.DeleteAll();

            foreach (var file in failedFiles)
            {
                await _mediator.Send(new SendFileToRecipientEvent { File = file.File, Recipient = file.Recipient });
            }

            return Unit.Value;
        }
    }
}
