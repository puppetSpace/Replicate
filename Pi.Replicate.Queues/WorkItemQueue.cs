using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Notification;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Pi.Replicate.Queueing
{
    internal sealed class WorkItemQueue<TE> : ConcurrentQueue<TE>, IWorkItemQueue<TE>
    {
        private readonly IWorkEventAggregator _workEventAggregator;
        private readonly QueueKind _queueKind;

        public WorkItemQueue(IWorkEventAggregator workEventAggregator, QueueKind queueKind)
        {
            _workEventAggregator = workEventAggregator;
            _queueKind = queueKind;
        }

        public Task<TE> Dequeue()
        {
            if (base.TryDequeue(out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult(default(TE));
        }

        public new Task Enqueue(TE item)
        {
            return Task.Factory.StartNew(x =>
            {
                var localItem = (TE)x;
                if (localItem != null)
                {
                    base.Enqueue(localItem);
                    _workEventAggregator.Publish(new WorkEventData { CurrentWorkload = base.Count, TypeOfWorkData = typeof(TE), QueueKind = _queueKind });
                }
            },item);

        }

        public bool HasItems()
        {
            return base.Count != 0;
        }
    }
}
