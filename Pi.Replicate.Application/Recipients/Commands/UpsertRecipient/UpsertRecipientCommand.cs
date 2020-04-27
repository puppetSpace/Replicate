using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Commands.UpsertRecipient
{
    public class UpsertRecipientCommand : IRequest
    {
        public string Name { get; set; }

        public string Address { get; set; }

		public bool Verified { get; set; }
	}

    public class UpsertRecipientCommandHandler : IRequestHandler<UpsertRecipientCommand>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = @"
			IF EXISTS(SELECT 1 FROM dbo.Recipient where Name = @Name)
				BEGIN
				UPDATE dbo.Recipient Set Address = @Address, Verified = @Verified where Name = @Name;
				END
			ELSE
				BEGIN
					INSERT INTO dbo.Recipient(Id,Name,Address,Verified) VALUES (@Id,@Name,@Address,@Verified);
				END";

        public UpsertRecipientCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(UpsertRecipientCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var builtRecipient = Recipient.Build(request.Name, request.Address,request.Verified);
                await _database.Execute(_insertStatement, new { builtRecipient.Id, builtRecipient.Name, builtRecipient.Address, builtRecipient.Verified }); ;

                return Unit.Value;
            }
        }
    }
}
