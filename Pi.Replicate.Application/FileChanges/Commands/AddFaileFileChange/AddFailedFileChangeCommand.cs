using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChanges.Commands.AddFaileFileChange
{
    public class AddFailedFileChangeCommand : IRequest
    {
        public FileChange FileChange { get; set; }

        public Recipient Recipient { get; set; }
    }

    public class AddFailedFileChangeCommandHandler : IRequestHandler<AddFailedFileChangeCommand>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = @"
            if not exists(select 1 from  dbo.FailedFileChange where FileChangeId = @FileChangeId and RecipientId = @RecipientId)
            begin
                INSERT INTO dbo.FailedFile(FileChangeId,RecipientId) VALUES (@FileChangeId,@RecipientId);
            end";

        public AddFailedFileChangeCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(AddFailedFileChangeCommand request, CancellationToken cancellationToken)
        {
            var failedFile = FailedFileChange.Build(request.FileChange, request.Recipient);
            using (_database)
                await _database.Execute(_insertStatement, new { FileChangeId = failedFile.FileChange.Id, RecipientId = failedFile.Recipient.Id });

            return Unit.Value;
        }
    }
}
