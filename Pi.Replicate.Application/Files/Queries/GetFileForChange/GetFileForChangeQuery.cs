using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Queries.GetFileForChange
{
    public class GetFileForChangeQuery : IRequest<File>
    {
        public string Path { get; set; }
    }

    public class GetFileForChangeQueryHandler : IRequestHandler<GetFileForChangeQuery, File>
    {
        private readonly IDatabase _database;
        private readonly IMapper _mapper;
        private const string _selectStatement = "SELECT Id, FolderId,AmountOfChunks, LastModifiedDate, Name,Path, Signature,Size,Status,Source FROM dbo.[File] WHERE Path = @Path";

        public GetFileForChangeQueryHandler(IDatabase database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }
        public async Task<File> Handle(GetFileForChangeQuery request, CancellationToken cancellationToken)
        {
            using (_database)
            {
                var result = await _database.QuerySingle<FileDto>(_selectStatement,new{request.Path});
                return _mapper.Map<File>(result);
            }
        }
    }
}