using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetFilesForFolder
{
    public class GetFilesForFolderQuery : IRequest<Result<ICollection<File>>>
    {
        public GetFilesForFolderQuery(Guid folderId)
        {
            FolderId = folderId;
        }

        public Guid FolderId { get; }
    }

    public class GetFilesForFolderQueryHandler : IRequestHandler<GetFilesForFolderQuery, Result<ICollection<File>>>
    {
        private readonly IDatabase _database;
        private readonly IMapper _mapper;
        private const string _selectStatement = @"
			select Id,FolderId,Name,Version,Size,LastModifiedDate,Path,Source
			from (
				select *,ROW_NUMBER() over(partition by Name order by Version desc)  rnk
				from dbo.[File]
				where folderId = @FolderId
			) a
			where rnk = 1
";

        public GetFilesForFolderQueryHandler(IDatabase database, IMapper mapper)
        {
            _database = database;
            _mapper = mapper;
        }

        public async Task<Result<ICollection<File>>> Handle(GetFilesForFolderQuery request, CancellationToken cancellationToken)
        {
			try
			{
				using (_database)
				{
					var result = await _database.Query<FileDao>(_selectStatement, new { request.FolderId });
					return Result<ICollection<File>>.Success(_mapper.Map<ICollection<File>>(result));
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query {nameof(GetFilesForFolderQuery)}");
				return Result<ICollection<File>>.Failure();
			}
        }
    }
}
