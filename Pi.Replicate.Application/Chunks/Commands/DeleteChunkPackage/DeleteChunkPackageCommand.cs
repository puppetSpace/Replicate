using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Commands.DeleteChunkPackage
{
    public class DeleteChunkPackageCommand : IRequest
    {
        public Guid ChunkPackageId { get; set; }
    }

    public class DeleteChunkPackageCommandHandler : IRequestHandler<DeleteChunkPackageCommand>
    {
        private readonly IDatabase _database;
        private const string _deleteStatement = "DELETE FROM dbo.ChunkPackages WHERE Id = @Id";

        public DeleteChunkPackageCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<Unit> Handle(DeleteChunkPackageCommand request, CancellationToken cancellationToken)
        {
            using (_database)
                await _database.Execute(_deleteStatement, new { Id = request.ChunkPackageId });

            return Unit.Value;
        }
    }
}
