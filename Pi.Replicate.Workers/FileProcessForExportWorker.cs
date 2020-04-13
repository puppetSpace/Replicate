using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Chunks.AddChunks;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Commands.UpdateFileAfterProcessing;
using Pi.Replicate.Application.Files.Queries.GetFilesForProcessing;
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
        private readonly int _triggerInterval;
        private readonly FileSplitterFactory _fileSplitterFactory;
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly IMediator _mediator;
        private readonly PathBuilder _pathBuilder;

        public FileProcessForExportWorker(IConfiguration configuration
        , FileSplitterFactory fileSplitterFactory
        , WorkerQueueFactory workerQueueFactory
        , IMediator mediator
        , PathBuilder pathBuilder)
        {
            _triggerInterval = int.TryParse(configuration[Constants.FileProcessForExportTriggerInterval], out int interval) ? interval : 10;
            _fileSplitterFactory = fileSplitterFactory;
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
                var outgoingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
                var fileSplitter = _fileSplitterFactory.Get();
                var runningTasks = new List<Task>();
                var semaphore = new SemaphoreSlim(10);
                while (!incomingQueue.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    runningTasks.Add(Task.Run(async () =>
                    {
                        await semaphore.WaitAsync();
                        var file = incomingQueue.Take(cancellationToken);
                        Log.Information($"Processing file '{file.Path}'");
                        int sequenceNo = 0;
                        if (file.Status == FileStatus.New)
                        {
                            var result = await fileSplitter.ProcessFile(file, async x => await _mediator.Send(new AddChunkCommand { Chunk = x, SequenceNo = ++sequenceNo, File = file }));
                            await _mediator.Send(new UpdateFileAfterProcessingCommand { File = file, Hash = result, AmountOfChunks = sequenceNo });
                        }
                        outgoingQueue.Add(file);
                        semaphore.Release();
                    }));

                    await Task.WhenAll(runningTasks);
                    Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min for next cycle");
                    await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
                }
            });

            thread.Start();
            return thread;
        }
    }
}
