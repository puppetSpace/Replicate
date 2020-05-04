using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.UpdateFile
{
	public class UpdateFileCommand : IRequest
	{
		public File File { get; set; }

		public ReadOnlyMemory<byte> Signature {get;set;}
    }

    public class UpdateFileHandler : IRequestHandler<UpdateFileCommand>
    {
        private readonly IDatabase _database;
        private const string _updateStatement = "UPDATE dbo.[File] SET Size = @Size,Version = @Version,LastModifiedDate = @LastModifiedDate where Id = @Id";
        private const string _updateStatementWithSignature = "UPDATE dbo.[File] SET Size = @Size,Version = @Version, LastModifiedDate = @LastModifiedDate, Signature = @Signature where Id = @Id";

        public UpdateFileHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                await _database.Execute(_updateStatementWithSignature, new { request.File.Id, request.File.Size, request.File.Version, request.File.LastModifiedDate, Signature = request.Signature.ToArray() });
            }

            return Unit.Value;
        }
    }
}
