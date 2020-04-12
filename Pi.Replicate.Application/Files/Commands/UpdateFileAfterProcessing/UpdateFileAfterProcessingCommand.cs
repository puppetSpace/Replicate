using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Commands.UpdateFileAfterProcessing
{
    public class UpdateFileAfterProcessingCommand : IRequest
    {
        public File File { get; set; }

        public byte[] Hash { get; set; }

        public int AmountOfChunks { get; set; }
    }

    public class UpdateFileAfterProcessingCommandHandler : IRequestHandler<UpdateFileAfterProcessingCommand>
    {
        private readonly IWorkerContext _workerContext;
        private readonly PathBuilder _pathBuilder;

        public UpdateFileAfterProcessingCommandHandler(IWorkerContext workerContext, PathBuilder pathBuilder )
        {
            _workerContext = workerContext;
            _pathBuilder = pathBuilder;
        }

        public async Task<Unit> Handle(UpdateFileAfterProcessingCommand request, CancellationToken cancellationToken)
        {
            request.File.UpdateAfterProcessesing(request.AmountOfChunks, request.Hash);
            _workerContext.Files.Update(request.File);
            await _workerContext.SaveChangesAsync(cancellationToken);

            if (request.File.Folder.FolderOptions.DeleteAfterSent)
            {
                var path = _pathBuilder.BuildPath(request.File);
                System.IO.File.Delete(path);
            }
            return Unit.Value;
        }
    }
}