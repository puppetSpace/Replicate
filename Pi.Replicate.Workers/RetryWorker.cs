using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Queries.GetFailedFiles;
using Pi.Replicate.Application.Files.Queries.GetFailedTransmissions;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class RetryWorker : WorkerBase
    {
        private readonly int _triggerInterval;
        private readonly IMediator _mediator;
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly TransmissionService _communicationService;

        public RetryWorker(IConfiguration configuration, IMediator mediator, WorkerQueueFactory workerQueueFactory, TransmissionService communicationService)
        {
            _triggerInterval = int.TryParse(configuration[Constants.RetryTriggerInterval], out var interval) ? interval : 10;
            _mediator = mediator;
            _workerQueueFactory = workerQueueFactory;
            _communicationService = communicationService;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                await RetryFailedFiles();
                await RetryFailedSentChunks();

                Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min to trigger retry logic again");
                await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
            });

            thread.Start();
            return thread;
        }

        private async Task RetryFailedFiles()
        {
            Log.Information($"Retrying to send files that have failed");
            var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
            var failedFiles = await _mediator.Send(new GetFailedFileTransmissionsCommand());
            foreach (var ff in failedFiles)
            {
                await _communicationService.SendFile(ff.Folder, ff.File, ff.Recipient);
                outgoingQueue.Add(ff.File);
            }
        }

        private async Task RetryFailedSentChunks()
        {
            var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
            Log.Information($"Retrying to send chunks that have failed");
            var chunkPackages = await _mediator.Send(new GetChunkPackagesQuery());

            foreach (var chunkPackage in chunkPackages)
            {
                if (!queue.Any(x => x.FileChunk.Id == chunkPackage.FileChunk.Id && x.Recipient.Id == chunkPackage.Recipient.Id))
                    queue.Add(chunkPackage);
            }
        }
    }
}
