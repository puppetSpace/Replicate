using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Folders.Commands.AddNewFolder
{
    public class AddNewFolderCommand : IRequest
    {
        public string Name { get; set; }

        public bool DeleteAfterSend { get; set; }

        public bool CreateOnDisk { get; set; }

        public List<Recipient> Recipients { get; set; }
    }

    public class AddNewFolderCommandHandler : IRequestHandler<AddNewFolderCommand>
    {
        private readonly IWorkerContext _workerContext;

        public AddNewFolderCommandHandler(IWorkerContext workerContext)
        {
            _workerContext = workerContext;
        }

        public async Task<Unit> Handle(AddNewFolderCommand request, CancellationToken cancellationToken)
        {
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                FolderOptions = new FolderOption { DeleteAfterSent = request.DeleteAfterSend },
            };

            folder.Recipients = request.Recipients.Select(x => new FolderRecipient { Folder = folder, Recipient = x }).ToList();

            _workerContext.Folders.Add(folder);
            await _workerContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
