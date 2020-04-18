using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Queries.GetChunkPackages
{
    public class GetChunkPackagesQuery : IRequest<ICollection<ChunkPackage>>
    {
        
    }

    public class GetChunkPackagesQueryHandler : IRequestHandler<GetChunkPackagesQuery, ICollection<ChunkPackage>>
    {
        private readonly IDatabase _database;
        private const string _selectStatement = @"
				select fc.Id, fc.FileId, fc.SequenceNo, fc.Value, fc.ChunkSource
				, re.Id, re.Name, re.Address
				from dbo.ChunkPackage cp
				inner join dbo.FileChunk fc on fc.Id = cp.FileChunkId
				inner join dbo.Recipient re on re.Id = cp.RecipientId";

        public GetChunkPackagesQueryHandler(IDatabase database)
        {
            _database = database;
        }


        public async Task<ICollection<ChunkPackage>> Handle(GetChunkPackagesQuery request, CancellationToken cancellationToken)
        {
            using(_database)
                return await _database.Query<ChunkPackage, FileChunk, Recipient, ChunkPackage>(_selectStatement,null, (cp, fc, re) =>
                {
                    cp.FileChunk = fc;
                    cp.Recipient = re;
                    return cp;
                });
        }
    }
}
