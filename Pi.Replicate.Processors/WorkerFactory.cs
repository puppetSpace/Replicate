using Microsoft.Extensions.Configuration;
using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.FileChunks;
using Pi.Replicate.Processing.Files;
using Pi.Replicate.Processing.Folders;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    public class WorkerFactory : IWorkerFactory
    {

        private readonly Dictionary<Tuple<Type, QueueKind>, Func<Worker>> _consumers = new Dictionary<Tuple<Type, QueueKind>, Func<Worker>>();
        private readonly Dictionary<QueueKind, Func<Worker>> _producers = new Dictionary<QueueKind, Func<Worker>>();

        public WorkerFactory(IWorkItemQueueFactory workItemQueueFactory, IRepository repository, IUploadLink uploadLink, IConfiguration configuration)
        {
            _consumers.Add(new Tuple<Type, QueueKind>(typeof(File), QueueKind.Incoming), () => new FileAssembler(workItemQueueFactory, repository, uploadLink));
            _consumers.Add(new Tuple<Type, QueueKind>(typeof(FileChunk), QueueKind.Incoming), () => new FileChunkDownload(workItemQueueFactory, repository, uploadLink));
            _consumers.Add(new Tuple<Type, QueueKind>(typeof(Folder), QueueKind.Outgoing), () => new FileCollector(workItemQueueFactory, repository));
            _consumers.Add(new Tuple<Type, QueueKind>(typeof(File), QueueKind.Outgoing), () => new FileSplitter(workItemQueueFactory, repository, configuration));
            _consumers.Add(new Tuple<Type, QueueKind>(typeof(FileChunk), QueueKind.Outgoing), () => new FileChunkUpload(workItemQueueFactory, repository, uploadLink));


            _producers.Add(QueueKind.Incoming, () => new FileChecker(workItemQueueFactory, repository, uploadLink));
            _producers.Add(QueueKind.Outgoing, () => new FolderWatcher(workItemQueueFactory, repository));
        }

        public Worker CreateConsumerWorker(Type typeOfWorkData, QueueKind queueKind)
        {
            var tuple = new Tuple<Type, QueueKind>(typeOfWorkData, queueKind);
            if (_consumers.ContainsKey(tuple))
                return _consumers[tuple].Invoke();

            throw new InvalidOperationException($"There is no working that consumes type {typeOfWorkData} for an {queueKind} queue");
        }

        public Worker CreateProducerWorker(QueueKind queueKind)
        {
            if (_producers.ContainsKey(queueKind))
                return _producers[queueKind].Invoke();

            throw new InvalidOperationException($"There is no producer of data for an {queueKind} queue");
        }
    }
}
