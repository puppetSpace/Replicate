using MediatR;
using Observr;
using Pi.Replicate.Application.Common;
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
        private readonly PathBuilder _pathBuilder;
        private readonly IBroker _broker;

        public AddNewFolderCommandHandler(IWorkerContext workerContext, PathBuilder pathBuilder, IBroker broker)
        {
            _workerContext = workerContext;
            _pathBuilder = pathBuilder;
            _broker = broker;
        }

        public async Task<Unit> Handle(AddNewFolderCommand request, CancellationToken cancellationToken)
        {
            var folder = new Folder
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                FolderOptions = new FolderOption { DeleteAfterSent = request.DeleteAfterSend },
            };

            folder.Recipients = request.Recipients.Select(x => new FolderRecipient { FolderId = folder.Id, RecipientId = x.Id }).ToList();

            _workerContext.Folders.Add(folder);
            await _workerContext.SaveChangesAsync(cancellationToken);

            var path = _pathBuilder.BuildPath(folder);
            if (request.CreateOnDisk && !System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);


            await _broker.Publish(folder);
            return Unit.Value;
        }
    }
}
