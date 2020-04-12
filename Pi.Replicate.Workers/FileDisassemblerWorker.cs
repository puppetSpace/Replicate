using MediatR;
using Pi.Replicate.Application.Chunks.AddChunks;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Files;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class FileDisassemblerWorker : WorkerBase
    {
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly FileSplitterFactory _fileSplitterFactory;
        private readonly IMediator _mediator;
        private readonly PathBuilder _pathBuilder;

        public FileDisassemblerWorker(WorkerQueueFactory workerQueueFactory
        , FileSplitterFactory fileSplitterFactory
        , IMediator mediator
        , PathBuilder pathBuilder)
        {
            _workerQueueFactory = workerQueueFactory;
            _fileSplitterFactory = fileSplitterFactory;
            _mediator = mediator;
            _pathBuilder = pathBuilder;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                Log.Information($"Starting {nameof(FileDisassemblerWorker)}");
                var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
                var fileSplitter = _fileSplitterFactory.Get();

                while (!queue.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    var file = queue.Take(cancellationToken);
                    int sequenceNo = 0;
                    Log.Information($"Taking file '{file.Path}' from queue");
                    if (file.Status == FileStatus.New)
                    {
                        var result = await fileSplitter.ProcessFile(file, async x => await _mediator.Send(new AddChunkCommand { Chunk = x, SequenceNo = ++sequenceNo, File = file }));
                        //todo update file and start sending it

                        if (file.Folder.FolderOptions.DeleteAfterSent)
                        {
                            var path = _pathBuilder.BuildPath(file);
                            System.IO.File.Delete(path);
                        }
                    }
                }
            });

            thread.Start();
            return thread;
        }
    }
}
