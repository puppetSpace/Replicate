using System.Threading;
using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using System.Net.Http;

namespace Pi.Replicate.Workers
{
    public class FileExportWorker : WorkerBase
    {
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly HttpClient _httpClient;
        private readonly IMediator _mediator;

        public FileExportWorker(WorkerQueueFactory workerQueueFactory, IMediator mediator, IHttpClientFactory clientFactory)
        {
            _workerQueueFactory = workerQueueFactory;
            _mediator = mediator;
            _httpClient = clientFactory.CreateClient();
        }
        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(() =>
            {
                var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
                while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
                {
                    var file = queue.Take();
                    //todo send to recipients
                    //create chunkpackages + add to queue
                }

            });

            thread.Start();
            return thread;
        }
    }
}