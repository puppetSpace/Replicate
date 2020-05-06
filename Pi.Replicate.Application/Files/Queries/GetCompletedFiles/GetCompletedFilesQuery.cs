﻿using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Queries.GetCompletedFiles
{
    public class GetCompletedFilesQuery : IRequest<Result<ICollection<CompletedFileDto>>>
    {
        
    }

	public class GetCompletedFilesQueryHandler : IRequestHandler<GetCompletedFilesQuery, Result<ICollection<CompletedFileDto>>>
	{
		private readonly IDatabase _database;
		private readonly IMapper _mapper;
		private const string _selectStatement = @"
			select fi.Id, fi.FolderId,fi.[Name],fi.[Version], fi.Size,fi.LastModifiedDate, fi.[Path], fi.Source
			, eme.Id,eme.FileId,eme.AmountOfChunks,eme.CreationTime
			from dbo.[File] fi
			inner join dbo.EofMessage eme on eme.FileId = fi.Id
			left join dbo.FileChunk fc on fc.FileId = fi.Id
			where fi.Source = 1
			group by fi.Id, fi.FolderId,fi.[Name],fi.[Version], fi.Size,fi.LastModifiedDate, fi.[Path], fi.Source
			, eme.Id,eme.FileId,eme.AmountOfChunks,eme.CreationTime
			having sum(fc.SequenceNo) = (eme.AmountOfChunks*(eme.AmountOfChunks+1)) / 2";

		public GetCompletedFilesQueryHandler(IDatabase database, IMapper mapper)
		{
			_database = database;
			_mapper = mapper;
		}

		public async Task<Result<ICollection<CompletedFileDto>>> Handle(GetCompletedFilesQuery request, CancellationToken cancellationToken)
		{
			try
			{
				using (_database)
				{
					var queryresult = await _database.Query<FileDao, EofMessage, CompletedFileDto>(_selectStatement, null
						, (f, e) => new CompletedFileDto { File = _mapper.Map<File>(f), EofMessage = e });
					return Result<ICollection<CompletedFileDto>>.Success(queryresult);
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing query '{nameof(GetCompletedFilesQuery)}'");
				return Result<ICollection<CompletedFileDto>>.Failure();
			}
		}
	}
}
