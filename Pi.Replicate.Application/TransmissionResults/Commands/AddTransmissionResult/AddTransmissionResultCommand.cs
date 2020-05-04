using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.TransmissionResults.Commands.AddTransmissionResult
{
    public class AddTransmissionResultCommand : IRequest
    {
        public Guid FileId { get; set; }

		public double FileChunkSequenceNo { get; set; }

        public Guid RecipientId { get; set; }
    }

    public class AddTransmissionResultCommandHandler : IRequestHandler<AddTransmissionResultCommand>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "INSERT INTO dbo.TransmissionResult(Id,RecipientId, FileId,FileChunkSequenceNo) VALUES(@Id,@RecipientId,@FileId, @FileChunkSequenceNo)";

        public AddTransmissionResultCommandHandler(IDatabase database)
        {
            _database = database;
        }
        public async Task<Unit> Handle(AddTransmissionResultCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                await _database.Execute(_insertStatement, new {Id=  Guid.NewGuid(), request.RecipientId,request.FileId,request.FileChunkSequenceNo });
            }

			return Unit.Value;
        }
    }
}