using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Queries.GetLatestVersionOfFile
{
    public class GetLatestVersionOfFileQuery : IRequest<File>
    {
        public string Path { get; set; }
		public Guid FolderId { get; set; }
	}

    public class GetLatestVersionOfFileQueryHandler : IRequestHandler<GetLatestVersionOfFileQuery, File>
    {
        private readonly IDatabase _database;
        private readonly IMapper _mapper;
        private const string _selectStatement = "SELECT TOP 1 Id, FolderId,Version, LastModifiedDate, Name,Path, Signature,Size,Source FROM dbo.[File] WHERE FolderId = @FolderId and Path = @Path order by Version desc";

        public GetLatestVersionOfFileQueryHandler(IDatabase database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }
        public async Task<File> Handle(GetLatestVersionOfFileQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var result = await _database.QuerySingle<FileDao>(_selectStatement,new{request.FolderId,request.Path});
                return _mapper.Map<File>(result);
            }
        }
    }
}