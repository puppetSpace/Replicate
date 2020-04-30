using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.EofMessages.Commands.AddEofMessage
{
    public class AddEofMessageCommand : IRequest
    {
        public EofMessage EofMessage { get; set; }
    }

    public class AddEofMessageCommandHandler : IRequestHandler<AddEofMessageCommand>
    {
        private readonly IDatabase _database;
		private const string _insertStatement = "INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)";

        public AddEofMessageCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(AddEofMessageCommand request, CancellationToken cancellationToken)
        {
            using (_database)
				await _database.Execute(_insertStatement,new {request.EofMessage.Id, request.EofMessage.FileId,request.EofMessage.AmountOfChunks});
        
			return Unit.Value;
		}
    }
}