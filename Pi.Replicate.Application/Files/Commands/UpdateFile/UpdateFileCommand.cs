using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.UpdateFile
{
    public class UpdateFileCommand : IRequest
    {
        public File File { get; set; }

        public bool AlsoUpdateSignature { get; set; }
    }

    public class UpdateFileHandler : IRequestHandler<UpdateFileCommand>
    {
        private readonly IDatabase _database;
        private const string _updateStatement = "UPDATE dbo.[File] SET Size = @Size,  AmountOfChunks = @AmountOfChunks, Status = @Status, LastModifiedDate = @LastModifiedDate where Id = @Id";
        private const string _updateStatementWithSignature = "UPDATE dbo.[File] SET Size = @Size,  AmountOfChunks = @AmountOfChunks, Status = @Status, LastModifiedDate = @LastModifiedDate, Signature = @Signature where Id = @Id";

        public UpdateFileHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                if(request.AlsoUpdateSignature)
                    await _database.Execute(_updateStatementWithSignature, new { request.File.Id, request.File.Size, request.File.AmountOfChunks, request.File.Status, request.File.LastModifiedDate, request.File.Signature });
                else
                    await _database.Execute(_updateStatement, new { request.File.Id, request.File.Size, request.File.AmountOfChunks, request.File.Status, request.File.LastModifiedDate });
            }


            return Unit.Value;
        }
    }
}
