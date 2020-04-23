using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChanges.Commands.AddFileChange
{
    public class AddFileChangeCommand : IRequest
    {
        public FileChange FileChange { get; set; }
    }

    public class AddFileChangeCommandHandler : IRequestHandler<AddFileChangeCommand>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "insert into dbo.FileChange(Id,FileId,VersionNo,AmountOfChunks,LastModifiedDate) values(@Id,@FileId,@VersionNo,@AmountOfChunks,@LastModifiedDate)";

        public AddFileChangeCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(AddFileChangeCommand request, CancellationToken cancellationToken)
        {
            using (_database)
                await _database.Execute(_insertStatement, new { request.FileChange.Id,request.FileChange.FileId, request.FileChange.VersionNo, request.FileChange.AmountOfChunks, request.FileChange.LastModifiedDate });

            return Unit.Value;
        }
    }
}
