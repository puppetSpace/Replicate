using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.TransmissionResults.Commands.AddTransmissionResult
{
    public class AddTransmissionResultCommand : IRequest
    {
        public FileChunk FileChunk { get; set; }

        public Recipient Recipient { get; set; }
    }

    public class AddTransmissionResultCommandHandler : IRequestHandler<AddTransmissionResultCommand>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "INSERT INTO dbo.TransmissionResult(RecipientId, FileChunkId) VALUES(@RecipientId,@FileChunkId)";

        public AddTransmissionResultCommandHandler(IDatabase database)
        {
            _database = database;
        }
        public async Task<Unit> Handle(AddTransmissionResultCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                await _database.Execute(_insertStatement, new { RecipientId = request.Recipient.Id, FileChunkId = request.FileChunk.Id });
            }

			return Unit.Value;
        }
    }
}