using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.EofMessages.Commands.AddReceivedEofMessage
{
    public class AddReceivedEofMessageCommand : IRequest
    {
		public Guid FileId { get; set; }

		public int AmountOfChunks { get; set; }
	}

	public class AddReceivedEofMessageCommandHandler : IRequestHandler<AddReceivedEofMessageCommand>
	{
		private readonly IDatabase _database;
		private const string _insertStatement = "INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)";

		public AddReceivedEofMessageCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(AddReceivedEofMessageCommand request, CancellationToken cancellationToken)
		{
			var eofMessage = EofMessage.Build(request.FileId, request.AmountOfChunks);
			using (_database)
				await _database.Execute(_insertStatement, new {eofMessage.Id, eofMessage.FileId, eofMessage.AmountOfChunks });

			return Unit.Value;
		}
	}
}
