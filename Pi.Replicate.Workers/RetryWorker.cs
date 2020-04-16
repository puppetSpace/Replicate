﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Chunks.Queries.GetChunkPackages;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Commands.SendFileToRecipient;
using Pi.Replicate.Application.Files.Queries.GetFailedFiles;
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

        public RetryWorker(IConfiguration configuration, IMediator mediator, WorkerQueueFactory workerQueueFactory)
        {
            _triggerInterval = int.TryParse(configuration[Constants.RetryTriggerInterval], out var interval) ? interval : 10;
            _mediator = mediator;
            _workerQueueFactory = workerQueueFactory;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                Log.Information($"Retrying to send files that have failed");
                var failedFiles = await _mediator.Send(new GetFailedFilesQuery());
                foreach (var file in failedFiles)
                {
                    await _mediator.Send(new SendFileToRecipientCommand { File = file.File, Recipient = file.Recipient });
                }

                var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
                Log.Information($"Retrying to send chunks that have failed");
                var chunkPackages = await _mediator.Send(new GetChunkPackagesQuery());

                foreach (var chunkPackage in chunkPackages)
                {
                    if (!queue.Any(x => x.Id == chunkPackage.Id))
                        queue.Add(chunkPackage);
                }

                Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min to trigger retry logic again");
                await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
            });

            thread.Start();
            return thread;
        }
    }
}
