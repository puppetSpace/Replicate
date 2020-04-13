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
    public class AddNewFilesCommand : IRequest<List<File>>
    {
        public AddNewFilesCommand(List<System.IO.FileInfo> newFiles, Folder folder)    
        {
            NewFiles = newFiles;
            Folder = folder;
        }

        public List<System.IO.FileInfo> NewFiles { get;}
        public Folder Folder { get; }
    }

    public class AddNewFilesCommandHandler : IRequestHandler<AddNewFilesCommand,List<File>>
    {
        private readonly IWorkerContext _workerContext;
        private readonly PathBuilder _pathBuilder;

        public AddNewFilesCommandHandler(IWorkerContext workerContext, PathBuilder pathBuilder)
        {
            _workerContext = workerContext;
            _pathBuilder = pathBuilder;
        }

        public async Task<List<File>> Handle(AddNewFilesCommand request, CancellationToken cancellationToken)
        {
            var createdFiles = new List<File>();
            foreach (var newFile in request.NewFiles)
            {
                Log.Verbose($"Adding '{newFile.FullName}' to context");
                var file = File.BuildPartial(newFile, request.Folder, _pathBuilder.BasePath);
                _workerContext.Files.Add(file);
                createdFiles.Add(file);
            }
            await _workerContext.SaveChangesAsync(cancellationToken);

            return createdFiles;
        }
    }
}
