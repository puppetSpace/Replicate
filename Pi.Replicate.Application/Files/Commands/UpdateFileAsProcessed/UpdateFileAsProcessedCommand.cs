using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.UpdateFileAsProcessed
{
    public class UpdateFileAsProcessedCommand : IRequest
    {
        public File File { get; set; }

        public byte[] Signature { get; set; }

        public int AmountOfChunks { get; set; }
    }

    public class UpdateFileAsProcessedCommandHandler : IRequestHandler<UpdateFileAsProcessedCommand>
    {
        private readonly IDatabase _database;
        private const string _updateStatement = "UPDATE dbo.[File] SET AmountOfChunks = @AmountOfChunks, Status = @Status, Signature = @Signature where Id = @Id";

        public UpdateFileAsProcessedCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(UpdateFileAsProcessedCommand request, CancellationToken cancellationToken)
        {
            request.File.UpdateAfterProcessesing(request.AmountOfChunks, request.Signature);

            using (_database)
               await  _database.Execute(_updateStatement, new { request.File.Id, request.File.AmountOfChunks, request.File.Status, request.File.Signature });


            return Unit.Value;
        }
    }
}
