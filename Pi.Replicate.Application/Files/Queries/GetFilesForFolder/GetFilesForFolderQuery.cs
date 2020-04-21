using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFilesForFolder
{
    public class GetFilesForFolderQuery : IRequest<ICollection<File>>
    {
        public GetFilesForFolderQuery(Guid folderId)
        {
            FolderId = folderId;
        }

        public Guid FolderId { get; }
    }

    public class GetFilesForFolderQueryHandler : IRequestHandler<GetFilesForFolderQuery, ICollection<File>>
    {
        private readonly IDatabase _database;
        private readonly IMapper _mapper;
        private const string _selectStatement = "SELECT Id, FolderId,AmountOfChunks, LastModifiedDate, Name,Path, Signature,Size,Status,Source FROM dbo.[File] WHERE FolderId = @FolderId";

        public GetFilesForFolderQueryHandler(IDatabase database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public async Task<ICollection<File>> Handle(GetFilesForFolderQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var result = await _database.Query<FileDto>(_selectStatement, new { request.FolderId });
                return _mapper.Map<ICollection<File>>(result);
            }
        }
    }
}
