using MediatR;
using Pi.Replicate.Application.Chunks.CreateChunkPackages;
using Pi.Replicate.Application.Chunks.Queries.GetChunksForFile;
using Pi.Replicate.Application.Chunks.Queries.GetChunksForFileChange;
using Pi.Replicate.Application.Files.Commands.UpdateFile;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
	internal class SendFileHandler
	{
		private readonly IMediator _mediator;
		private readonly CommunicationService _communicationService;

		public SendFileHandler(IMediator mediator, CommunicationService communicationService)
		{
			_mediator = mediator;
			_communicationService = communicationService;
		}

		public async Task Handle(File file, Recipient recipient, BlockingCollection<ChunkPackage> outgoingQueue)
		{
			var sucess = await _communicationService.SendFile(file, recipient);
			if (sucess)
			{
				await CreateChunkPackages(file, recipient, outgoingQueue);
				file.MarkAsHandled();
				await _mediator.Send(new UpdateFileCommand { File = file });
			}
		}

		public async Task Handle(FileChange fileChange, Recipient recipient, BlockingCollection<ChunkPackage> outgoingQueue)
		{
			var sucess = await _communicationService.SendFileChange(fileChange, recipient);
			if (sucess)
			{
				await CreateChunkPackages(fileChange, recipient, outgoingQueue);
				fileChange.File.MarkAsHandled();
				await _mediator.Send(new UpdateFileCommand { File = fileChange.File });
			}
		}

		private async Task CreateChunkPackages(File file, Recipient recipient, BlockingCollection<ChunkPackage> outgoingQueue)
		{
			int processedChunks = 0;
			while (processedChunks < file.AmountOfChunks)
			{
				var chunks = await _mediator.Send(new GetChunksForFileQuery { FileId = file.Id, MinSequenceNo = processedChunks, MaxSequenceNo = 100 });
				processedChunks += chunks.Count;
				var builtChunkPackages = await _mediator.Send(new CreateChunkPackagesCommand { FileChunks = chunks, Recipient = recipient });
				foreach (var package in builtChunkPackages)
					outgoingQueue.Add(package);
			}
		}

		private async Task CreateChunkPackages(FileChange fileChange, Recipient recipient, BlockingCollection<ChunkPackage> outgoingQueue)
		{
			//todo check if you can work in passes here like with the new files
			var chunks = await _mediator.Send(new GetChunksForFileChangeQuery { FileId = fileChange.File.Id, FileChangeVersionNo = fileChange.VersionNo });
			var builtChunkPackages = await _mediator.Send(new CreateChunkPackagesCommand { FileChunks = chunks, Recipient = recipient });
			foreach (var package in builtChunkPackages)
				outgoingQueue.Add(package);
		}
	}
}
