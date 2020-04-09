using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddNewFiles
{
    public class AddNewFilesCommand : IRequest
    {
        public AddNewFilesCommand(List<System.IO.FileInfo> newFiles, Folder folder)    
        {
            NewFiles = newFiles;
            Folder = folder;
        }

        public List<System.IO.FileInfo> NewFiles { get;}
        public Folder Folder { get; }
    }

    public class AddNewFileToQueueCommandHandler : IRequestHandler<AddNewFilesCommand>
    {
        private readonly IWorkerContext _workerContext;
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly PathBuilder _pathBuilder;

        public AddNewFileToQueueCommandHandler(IWorkerContext workerContext, WorkerQueueFactory workerQueueFactory, PathBuilder pathBuilder)
        {
            _workerContext = workerContext;
            _workerQueueFactory = workerQueueFactory;
            _pathBuilder = pathBuilder;
        }

        public async Task<Unit> Handle(AddNewFilesCommand request, CancellationToken cancellationToken)
        {
            var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
            foreach (var newFile in request.NewFiles)
            {
                Log.Verbose($"Adding '{newFile.FullName}' to context");
                var file = File.BuildPartial(newFile, request.Folder, _pathBuilder.BasePath);
                _workerContext.Files.Add(file);

                Log.Verbose($"Adding '{newFile.FullName}' to queue");
                if (queue.GetConsumingEnumerable().Any(x => string.Equals(x.Path, file.Path)))
                    Log.Information($"{newFile.FullName} already present in queue for processing");
                else
                    queue.Add(file);
            }
            await _workerContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
