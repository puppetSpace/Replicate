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
        private readonly HttpHelper _httpHelper;
        private readonly IMediator _mediator;

        public FileExportWorker(WorkerQueueFactory workerQueueFactory, IMediator mediator, IHttpClientFactory clientFactory)
        {
            _workerQueueFactory = workerQueueFactory;
            _mediator = mediator;
            _httpHelper = new HttpHelper(clientFactory);
        }
        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var thread = new Thread(async () =>
            {
                var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToSendFiles);
                while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
                {
                    var file = queue.Take();

                    foreach (var recipient in file.Folder.Recipients)
                    {
                        var endpoint = $"{recipient.Recipient.Address}/api/file";
                        try
                        {
                            await _httpHelper.Post(endpoint, file);
                            //todo create chunkpackge
                            //todo add to queue
                        }
                        catch (System.InvalidOperationException ex)
                        {
                            //add to failed files
                            throw;
                        }
                    }
                }

            });

            thread.Start();
            return thread;
        }
    }
}