using System.Threading;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;

namespace Pi.Replicate.Workers
{
    public class FileExportWorker : WorkerBase
    {
        private readonly WorkerQueueFactory _workerQueueFactory;

        public FileExportWorker(WorkerQueueFactory workerQueueFactory)
        {
            _workerQueueFactory = workerQueueFactory;
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