using MediatR;
using Pi.Replicate.Application.FileChanges.Commands.AddFileChange;
using Pi.Replicate.Application.FileChanges.Queries.GetHighestChangeVersionNo;
using Pi.Replicate.Application.Files.Commands.UpdateFile;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
	//todo errorhandling
	public class FileService
	{
		private readonly ChunkService _chunkService;
		private readonly PathBuilder _pathBuilder;
		private readonly IMediator _mediator;

		public FileService(ChunkService chunkService, PathBuilder pathBuilder, IMediator mediator)
		{
			_chunkService = chunkService;
			_pathBuilder = pathBuilder;
			_mediator = mediator;
		}

		public async Task ProcessNewFile(File processItem)
		{
			var result = await _chunkService.SplitFileIntoChunks(processItem);
			if (result.isSucessfullySplitted)
			{
				var deltaservice = new DeltaService();
				var signature = deltaservice.CreateSignature(_pathBuilder.BuildPath(processItem.Path));
				processItem.SetAmountOfChunks(result.amountOfChunks);
				processItem.SetSignature(signature);
				processItem.MarkAsProcessed();
				await _mediator.Send(new UpdateFileCommand { File = processItem, AlsoUpdateSignature = true });
			}
		}

		public async Task<FileChange> ProcessChangedFile(File processItem)
		{
			var path = _pathBuilder.BuildPath(processItem.Path);
			var deltaservice = new DeltaService();
			var delta = deltaservice.CreateDelta(path, processItem.Signature);
			var newSignature = deltaservice.CreateSignature(path);
			var highestVersionNo = await _mediator.Send(new GetHighestChangeVersionNoQuery { FileId = processItem.Id });
			highestVersionNo += 1;
			var amountOfChunks = await _chunkService.SplitMemoryIntoChunks(processItem, highestVersionNo, delta);
			var fileChange = FileChange.Build(processItem, highestVersionNo, amountOfChunks, processItem.LastModifiedDate);
			await _mediator.Send(new AddFileChangeCommand { FileChange = fileChange });

			processItem.SetSignature(newSignature);
			processItem.MarkAsProcessed();
			await _mediator.Send(new UpdateFileCommand { File = processItem, AlsoUpdateSignature = true });
			return fileChange;

		}

	}
}
