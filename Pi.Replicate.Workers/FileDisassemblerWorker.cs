﻿using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Files;
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

        public FileDisassemblerWorker(WorkerQueueFactory workerQueueFactory, FileSplitterFactory fileSplitterFactory, IMediator mediator)
        {
            _workerQueueFactory = workerQueueFactory;
            _fileSplitterFactory = fileSplitterFactory;
            _mediator = mediator;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
                var fileSplitter = _fileSplitterFactory.Get();

                while (!queue.IsCompleted && !cancellationToken.IsCancellationRequested)
                {
                    var file = queue.Take(cancellationToken);
                    if(file.Status == FileStatus.New)
                    {
                        var result = await fileSplitter.ProcessFile(file);

                    }
                }
            });

            thread.Start();
            return thread;
        }
    }
}
