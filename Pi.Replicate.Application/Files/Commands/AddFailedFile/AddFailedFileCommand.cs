using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddFailedFile
{
    public class AddFailedFileCommand : IRequest
    {
        public File File { get; set; }

        public Recipient Recipient { get; set; }
    }

    public class AddFailedFileCommandHandler : IRequestHandler<AddFailedFileCommand>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "INSERT INTO dbo.FailedFile(FileId,RecipientId) VALUES (@FileId,@RecipientId)";

        public AddFailedFileCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(AddFailedFileCommand request, CancellationToken cancellationToken)
        {
            var failedFile = FailedFile.Build(request.File, request.Recipient);
            using (_database)
                await _database.Execute(_insertStatement, new {FileId = failedFile.File.Id, RecipientId = failedFile.Recipient.Id });

            return Unit.Value;
        }
    }
}
