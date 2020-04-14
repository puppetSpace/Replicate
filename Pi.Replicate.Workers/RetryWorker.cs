using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Chunks.Events.RetryFailedChunks;
using Pi.Replicate.Application.Files.Events.RetryFailedFiles;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class RetryWorker : WorkerBase
    {
        private readonly int _triggerInterval;
        private readonly IMediator _mediator;

        public RetryWorker(IConfiguration configuration, IMediator mediator)
        {
            _triggerInterval = int.TryParse(configuration[Constants.RetryTriggerInterval], out var interval) ? interval : 10;
            _mediator = mediator;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                Log.Information($"Retrying to send files that have failed");
                await _mediator.Send(new RetryFailedFilesEvent());

                Log.Information($"Retrying to send chunks that have failed");
                await _mediator.Send(new RetryFailedChunksEvent());

                Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min to trigger retry logic again");
                await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
            });

            thread.Start();
            return thread;
        }
    }
}
