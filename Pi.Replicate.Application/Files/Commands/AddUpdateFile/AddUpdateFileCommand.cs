using AutoMapper;
using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Models;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddUpdateFile
{
	public class AddUpdateFileCommand : IRequest<Result<File>>
	{
		public System.IO.FileInfo FileInfo { get; set; }

		public Guid FolderId { get; set; }

		public ReadOnlyMemory<byte> Signature { get; set; }
	}

	public class AddUpdateFileCommandHandler : IRequestHandler<AddUpdateFileCommand, Result<File>>
	{
		private readonly IDatabase _database;
		private readonly IMapper _mapper;
		private readonly PathBuilder _pathBuilder;
		private const string _insertStatement = "INSERT INTO dbo.[File](Id,FolderId, Name, Size,Version,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @Version, @LastModifiedDate,@Path, @Signature, @Source)";
		private const string _selectStatement = "SELECT TOP 1 Id, FolderId,Version, LastModifiedDate, Name,Path,Size,Source FROM dbo.[File] WHERE FolderId = @FolderId and Path = @Path order by Version desc";

		public AddUpdateFileCommandHandler(IDatabase database, IMapper mapper, PathBuilder pathBuilder)
		{
			_database = database;
			_mapper = mapper;
			_pathBuilder = pathBuilder;
		}

		public async Task<Result<File>> Handle(AddUpdateFileCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var relativePath = request.FileInfo.FullName.Replace(_pathBuilder.BasePath + "\\", "");
				var result = await _database.QuerySingle<FileDao>(_selectStatement, new { request.FolderId, Path = relativePath });
				var foundFile = _mapper.Map<File>(result);
				if (foundFile is object)
				{
					foundFile.Update(request.FileInfo);
					await _database.Execute(_insertStatement, new { foundFile.Id, foundFile.FolderId, foundFile.Name, foundFile.Size, foundFile.Version, foundFile.LastModifiedDate, foundFile.Path, Signature = request.Signature.ToArray(), foundFile.Source });
				}
				return Result<File>.Success(foundFile);
			}
		}
	}
}
