using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Events.SplitFile;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Files;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class FileProcessForExportWorker : WorkerBase
    {
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly IMediator _mediator;
        private readonly PathBuilder _pathBuilder;

        public FileProcessForExportWorker(IConfiguration configuration
        , WorkerQueueFactory workerQueueFactory
        , IMediator mediator
        , PathBuilder pathBuilder)
        {
            _workerQueueFactory = workerQueueFactory;
            _mediator = mediator;
            _pathBuilder = pathBuilder;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                Log.Information($"Starting {nameof(FileProcessForExportWorker)}");
                var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
                var runningTasks = new List<Task>();
                var semaphore = new SemaphoreSlim(10);
                while (!incomingQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    runningTasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        var file = incomingQueue.Take(cancellationToken);
                        Log.Information($"Processing file '{file.Path}'");
                        await _mediator.Send(new SplitFileEvent { File = file });
                        semaphore.Release();
                    }));

                    await Task.WhenAll(runningTasks);
                }
            });

            thread.Start();
            return thread;
        }
    }
}
