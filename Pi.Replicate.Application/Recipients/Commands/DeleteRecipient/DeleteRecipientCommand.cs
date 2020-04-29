using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Commands.DeleteRecipient
{
    public class DeleteRecipientCommand : IRequest
    {
		public string Name { get; set; }
	}

	public class DeleteRecipientCommandHandler : IRequestHandler<DeleteRecipientCommand>
	{
		private readonly IDatabase _database;
		private const string _deleteStatement = "DELETE FROM dbo.Recipient WHERE Name = @Name";

		public DeleteRecipientCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(DeleteRecipientCommand request, CancellationToken cancellationToken)
		{
			using (_database)
				await _database.Execute(_deleteStatement, new { request.Name });

			return Unit.Value;
		}
	}
}
