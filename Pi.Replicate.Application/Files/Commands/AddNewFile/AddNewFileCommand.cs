using MediatR;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddNewFile
{
    public class AddNewFileCommand : IRequest<Result<File>>
    {
        public System.IO.FileInfo FileInfo { get; set; }

		public Guid FolderId { get; set; }

		public ReadOnlyMemory<byte> Signature { get; set; }
	}

    public class AddNewFileCommandHandler : IRequestHandler<AddNewFileCommand, Result<File>>
    {
        private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;
		private const string _insertStatement = "INSERT INTO dbo.[File](Id,FolderId, Name, Size,Version,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @Version, @LastModifiedDate,@Path, @Signature, @Source)";

        public AddNewFileCommandHandler(IDatabase database, PathBuilder pathBuilder)
        {
            _database = database;
			_pathBuilder = pathBuilder;
		}

        public async Task<Result<File>> Handle(AddNewFileCommand request, CancellationToken cancellationToken)
        {
			try
			{
				var file = File.Build(request.FileInfo, request.FolderId, _pathBuilder.BasePath);
				using (_database)
					await _database.Execute(_insertStatement, new { file.Id, file.FolderId, file.Name, file.Size, file.Version, file.LastModifiedDate, file.Path, Signature = request.Signature.ToArray(), file.Source });

				return Result<File>.Success(file);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while executing command '{nameof(AddNewFileCommand)}'");
				return Result<File>.Failure();
			}
        }
    }
}
