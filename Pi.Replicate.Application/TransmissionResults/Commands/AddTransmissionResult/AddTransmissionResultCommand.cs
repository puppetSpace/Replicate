using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.TransmissionResults.Commands.AddTransmissionResult
{
    public class AddTransmissionResultCommand : IRequest<Result>
    {
        public Guid FileId { get; set; }

		public double FileChunkSequenceNo { get; set; }

        public Guid RecipientId { get; set; }

		public FileSource Source { get; set; }
	}


    public class AddTransmissionResultCommandHandler : IRequestHandler<AddTransmissionResultCommand, Result>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "INSERT INTO dbo.TransmissionResult(Id,RecipientId, FileId,FileChunkSequenceNo, Source) VALUES(NEWID(),@RecipientId,@FileId, @FileChunkSequenceNo, @Source)";

        public AddTransmissionResultCommandHandler(IDatabase database)
        {
            _database = database;
        }
        public async Task<Result> Handle(AddTransmissionResultCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                await _database.Execute(_insertStatement, new {request.RecipientId,request.FileId,request.FileChunkSequenceNo, request.Source });
            }

			return Result.Success();
        }
    }
}