using MediatR;
using Pi.Replicate.Application.Chunks.CreateChunkPackages;
using Pi.Replicate.Application.Chunks.Queries.GetForFile;
using Pi.Replicate.Application.Files.Commands.UpdateFileAsHandled;
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
                await _mediator.Send(new UpdateFileAsHandledCommand { File = file });
            }
        }

        private async Task CreateChunkPackages(File file, Recipient recipient, BlockingCollection<ChunkPackage> outgoingQueue)
        {
            int processedChunks = 0;
            while (processedChunks < file.AmountOfChunks)
            {
                var chunks = await _mediator.Send(new GetForFileQuery { FileId = file.Id, MinSequenceNo = processedChunks, MaxSequenceNo = 100 });
                processedChunks += chunks.Count;
                var builtChunkPackages = await _mediator.Send(new CreateChunkPackagesCommand { FileChunks = chunks, Recipient = recipient });
                foreach (var package in builtChunkPackages)
                    outgoingQueue.Add(package);
            }
        }
    }
}
