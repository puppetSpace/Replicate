using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    public class WorkerFactory : IWorkerFactory
    {
        public Worker CreateConsumerWorker(Type typeOfWorkData, QueueKind queueKind)
        {
            throw new NotImplementedException();
        }

        public Worker CreateProducerWorker(QueueKind queueKind)
        {
            throw new NotImplementedException();
        }
    }
}
