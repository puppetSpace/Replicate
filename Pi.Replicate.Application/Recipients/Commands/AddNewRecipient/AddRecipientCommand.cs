using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Recipients.Commands.AddRecipient
{
    public class AddRecipientCommand : IRequest<Recipient>
    {
        public string Name { get; set; }

        public string Address { get; set; }
    }

    public class AddRecipientCommandHandler : IRequestHandler<AddRecipientCommand, Recipient>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "INSERT INTO dbo.Recipient(Id,Name,Address) VALUES(@Id,@Name,@Address)";

        public AddRecipientCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Recipient> Handle(AddRecipientCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var builtRecipient = Recipient.Build(request.Name, request.Address);
                await _database.Execute(_insertStatement, new { builtRecipient.Id, builtRecipient.Name, builtRecipient.Address }); ;

                return builtRecipient;
            }
        }
    }
}
