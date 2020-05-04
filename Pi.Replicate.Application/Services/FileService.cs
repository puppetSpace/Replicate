using MediatR;
using Pi.Replicate.Application.Files.Commands.AddNewFile;
using Pi.Replicate.Application.Files.Commands.UpdateFile;
using Pi.Replicate.Application.Files.Queries.GetLatestVersionOfFile;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	//todo errorhandling
	public class FileService
	{
		private readonly DeltaService _deltaService;
		private readonly PathBuilder _pathBuilder;
		private readonly IMediator _mediator;

		public FileService(DeltaService deltaService, PathBuilder pathBuilder, IMediator mediator)
		{
			_deltaService = deltaService;
			_pathBuilder = pathBuilder;
			_mediator = mediator;
		}

		public async Task<File> CreateNewFile(Folder folder, System.IO.FileInfo newFile)
		{
			var signature = _deltaService.CreateSignature(newFile.FullName);
			var file = File.Build(newFile, folder.Id, _pathBuilder.BasePath);
			await _mediator.Send(new AddNewFileCommand { File = file, Signature = signature });
			return file;
		}

		public async Task<File> CreateUpdateFile(Folder folder, System.IO.FileInfo changedFile)
		{
			var relativePath = changedFile.FullName.Replace(_pathBuilder.BasePath + "\\", "");
			var foundFile = await _mediator.Send(new GetLatestVersionOfFileQuery { FolderId = folder.Id, Path = relativePath });
			if (foundFile is object)
			{
				var signature = _deltaService.CreateSignature(changedFile.FullName);
				foundFile.Update(changedFile);
				await _mediator.Send(new UpdateFileCommand { File = foundFile, Signature = signature });
			}

			return foundFile;
		}

	}
}
