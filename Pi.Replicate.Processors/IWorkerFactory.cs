using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing
{
    public interface IWorkerFactory
    {
        Worker CreateProducerWorker(QueueKind queueKind);

        Worker CreateConsumerWorker(Type typeOfWorkData, QueueKind queueKind);
    }
}
