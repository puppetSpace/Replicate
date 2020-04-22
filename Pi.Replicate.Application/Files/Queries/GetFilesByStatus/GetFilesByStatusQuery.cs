using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFilesByStatus
{
    public class GetFilesByStatusQuery : IRequest<ICollection<File>>
    {
		public FileStatus Status { get; set; }
	}

    public class GetFilesByStatusQueryHandler : IRequestHandler<GetFilesByStatusQuery, ICollection<File>>
    {
        private readonly IDatabase _database;
		private readonly IMapper _mapper;
		private const string _selectStatement = @"SELECT Id, FolderId,AmountOfChunks, LastModifiedDate, Name,Path, Signature,Size,Status,Source FROM dbo.[File] WHERE Status = @Status";


		public GetFilesByStatusQueryHandler(IDatabase database, IMapper mapper)
        {
            _database = database;
			_mapper = mapper;
		}

        public async Task<ICollection<File>> Handle(GetFilesByStatusQuery request, CancellationToken cancellationToken)
        {
			using (_database)
			{
				var result = await _database.Query<FileDto>(_selectStatement, new { request.Status });
				return _mapper.Map<ICollection<File>>(result);
			}
        }
    }
}
