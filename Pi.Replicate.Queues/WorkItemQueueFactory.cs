using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Queueing
{
    public sealed class WorkItemQueueFactory : IWorkItemQueueFactory
    {
        private readonly WorkItemQueueStore _workItemQueueStore;

        public WorkItemQueueFactory(IWorkEventAggregator workEventAggregator)
        {
            _workItemQueueStore = new WorkItemQueueStore(workEventAggregator);
        }

        public IWorkItemQueue<TE> GetQueue<TE>(QueueKind queueKind)
        {
            return _workItemQueueStore.GetOrCreate<TE>(queueKind);
        }
    }
}
