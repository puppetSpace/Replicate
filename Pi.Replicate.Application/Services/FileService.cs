using MediatR;
using Pi.Replicate.Application.Files.Commands.AddNewFile;
using Pi.Replicate.Application.Files.Commands.AddUpdateFile;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	public class FileService
	{
		private readonly DeltaService _deltaService;
		private readonly IMediator _mediator;

		public FileService(DeltaService deltaService, IMediator mediator)
		{
			_deltaService = deltaService;
			_mediator = mediator;
		}

		public async Task<File> CreateNewFile(Folder folder, System.IO.FileInfo newFile)
		{
			var signature = _deltaService.CreateSignature(newFile.FullName);
			var result = await _mediator.Send(new AddNewFileCommand { FileInfo = newFile, FolderId = folder.Id, Signature = signature });
			return result.WasSuccessful ? result.Data : null;
		}

		public async Task<File> CreateUpdateFile(Folder folder, System.IO.FileInfo changedFile)
		{
			var signature = _deltaService.CreateSignature(changedFile.FullName);
			var result = await _mediator.Send(new AddUpdateFileCommand { FileInfo = changedFile, FolderId = folder.Id, Signature = signature });
			return result.WasSuccessful ? result.Data : null;
		}

	}
}
