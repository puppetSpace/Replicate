using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.EofMessages.Commands.AddToSendEofMessage
{
    public class AddToSendEofMessageCommand : IRequest<EofMessage>
    {
		public Guid FileId { get; set; }
		public int AmountOfChunks { get; set; }
	}

    public class AddToSendSentEofMessageCommandHandler : IRequestHandler<AddToSendEofMessageCommand, EofMessage>
    {
        private readonly IDatabase _database;
		private const string _insertStatement = "INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)";

        public AddToSendSentEofMessageCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<EofMessage> Handle(AddToSendEofMessageCommand request, CancellationToken cancellationToken)
        {
			var eofMessage = EofMessage.Build(request.FileId, request.AmountOfChunks);
            using (_database)
				await _database.Execute(_insertStatement,new {eofMessage.Id, eofMessage.FileId,eofMessage.AmountOfChunks});
        
			return eofMessage;
		}
    }
}