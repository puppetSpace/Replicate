using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.UpdateFileAsHandled
{
	public class UpdateFileAsHandledCommand : IRequest
	{
		public File File { get; set; }
	}

	public class UpdateFileAsHandledCommandHandler : IRequestHandler<UpdateFileAsHandledCommand>
	{
		private readonly IDatabase _database;
		private const string _updateStatement = "UPDATE dbo.Files SET Status = @Status WHERE Id = @Id";

		public UpdateFileAsHandledCommandHandler(IDatabase database)
		{
			_database = database;
		}

		public async Task<Unit> Handle(UpdateFileAsHandledCommand request, CancellationToken cancellationToken)
		{
			request.File.MarkAsHandled();
			using (_database)
			{
				await _database.Execute(_updateStatement, new { request.File.Id, request.File.Status });
			}

			return Unit.Value;
		}
	}
}
