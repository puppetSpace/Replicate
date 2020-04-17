using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.CreateChunkPackages
{
    public class CreateChunkPackagesCommand : IRequest<ICollection<ChunkPackage>>
    {
        public ICollection<FileChunk> FileChunks { get; set; }

        public Recipient Recipient { get; set; }
    }

    public class CreateChunkPackagesCommandHandler : IRequestHandler<CreateChunkPackagesCommand, ICollection<ChunkPackage>>
    {
        private readonly IDatabase _database;
        private const string _insertStatement = "INSERT INTO dbo.ChunkPackages(Id,FileChunkId,RecipientId) VALUES(@Id,@FileChunkId,@RecipientId";

        public CreateChunkPackagesCommandHandler(IDatabase database)
        {
            _database = database;
        }

        public async Task<ICollection<ChunkPackage>> Handle(CreateChunkPackagesCommand request, CancellationToken cancellationToken)
        {
            var builtChunkPackages = new List<ChunkPackage>();
            using (_database)
            {

                foreach (var chunk in request.FileChunks)
                {
                    var package = ChunkPackage.Build(chunk, request.Recipient);
                    await _database.Execute(_insertStatement, new { package.Id, FileChunkId = package.FileChunk.Id, RecipientId = package.Recipient.Id });
                    builtChunkPackages.Add(package);
                }
            }

            return builtChunkPackages;
        }
    }
}
