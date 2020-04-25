using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddNewFile
{
    public class AddNewFileCommand : IRequest
    {
        public File File { get; set; }
    }

    public class AddNewFileCommandHandler : IRequestHandler<AddNewFileCommand>
    {
        private readonly IDatabase _database;
        private readonly PathBuilder _pathBuilder;
        private const string _insertStatement = "INSERT INTO dbo.[File](Id,FolderId, Name, Size,Version,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @Version, @LastModifiedDate,@Path, @Signature, @Source)";

        public AddNewFileCommandHandler(IDatabase database, PathBuilder pathBuilder)
        {
            _database = database;
            _pathBuilder = pathBuilder;
        }

        public async Task<Unit> Handle(AddNewFileCommand request, CancellationToken cancellationToken)
        {
            using (_database)
                await _database.Execute(_insertStatement, new { request.File.Id, request.File.FolderId, request.File.Name, request.File.Size, request.File.Version, request.File.LastModifiedDate, request.File.Path, Signature = request.File.Signature.ToArray(), request.File.Source });

            return Unit.Value;
        }
    }
}
