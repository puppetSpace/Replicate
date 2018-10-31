using Pi.Replicate.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Queueing
{
    public sealed class WorkItemQueueFactory : IWorkItemQueueFactory
    {
        private readonly WorkItemQueueStore _workItemQueueStore = new WorkItemQueueStore();

        public IWorkItemQueue<TE> GetQueue<TE>(QueueKind queueKind)
        {
            return _workItemQueueStore.GetOrCreate<TE>(queueKind);
        }
    }
}
