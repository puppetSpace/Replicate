using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.AddNewFiles
{
	public class AddNewFilesCommand : IRequest<List<File>>
	{
		public AddNewFilesCommand(List<System.IO.FileInfo> newFiles, Folder folder)
		{
			NewFiles = newFiles;
			Folder = folder;
		}

		public List<System.IO.FileInfo> NewFiles { get; }
		public Folder Folder { get; }
	}

	public class AddNewFilesCommandHandler : IRequestHandler<AddNewFilesCommand, List<File>>
	{
		private readonly IDatabase _database;
		private readonly PathBuilder _pathBuilder;
		private const string _insertStatement = "INSERT INTO dbo.[File](Id,FolderId, Name, Size,AmountOfChunks,Status,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @AmountOfChunks, @Status, @LastModifiedDate,@Path, @Signature, @Source)";

		public AddNewFilesCommandHandler(IDatabase database, PathBuilder pathBuilder)
		{
			_database = database;
			_pathBuilder = pathBuilder;
		}

		//todo don't like this
		public async Task<List<File>> Handle(AddNewFilesCommand request, CancellationToken cancellationToken)
		{
			using (_database)
			{
				var createdFiles = new List<File>();
				foreach (var newFile in request.NewFiles)
				{
					Log.Information($"Creating signature for delta-calculations of file '{newFile.FullName}'");
					var deltaservice = new DeltaService();
					var signature = deltaservice.CreateSignature(newFile.FullName);

					Log.Information($"Adding file '{newFile.FullName}' in database");
					var file = File.BuildPartial(newFile, request.Folder.Id, _pathBuilder.BasePath, signature);
					await _database.Execute(_insertStatement, new { file.Id, file.FolderId, file.Name, file.Size, file.AmountOfChunks, file.Status, file.LastModifiedDate, file.Path, file.Signature, file.Source });
					createdFiles.Add(file);

				}
				return createdFiles;
			}
		}
	}
}
