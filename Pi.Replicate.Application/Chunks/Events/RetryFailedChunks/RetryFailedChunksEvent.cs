using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.Events.RetryFailedChunks
{
    public class RetryFailedChunksEvent : IRequest
    {
        
    }

    public class RetryFailedChunksEventHandler : IRequestHandler<RetryFailedChunksEvent>
    {
        private readonly IWorkerContext _workerContext;
        private readonly WorkerQueueFactory _workerQueueFactory;

        public RetryFailedChunksEventHandler(IWorkerContext workerContext, WorkerQueueFactory workerQueueFactory)
        {
            _workerContext = workerContext;
            _workerQueueFactory = workerQueueFactory;
        }

        public Task<Unit> Handle(RetryFailedChunksEvent request, CancellationToken cancellationToken)
        {
            var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
            foreach (var chunkPackage in _workerContext.ChunkPackages.AsNoTracking())
            {
                if (!queue.GetConsumingEnumerable().Any(x => x.Id == chunkPackage.Id))
                    queue.Add(chunkPackage);
            }

            return Unit.Task;
        }
    }
}
