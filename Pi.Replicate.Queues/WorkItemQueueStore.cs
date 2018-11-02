using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;



namespace Pi.Replicate.Queueing
{
    internal sealed class WorkItemQueueStore
    {
        private readonly HashSet<StoreItem> _store = new HashSet<StoreItem>();
        private readonly object _locker = new object();
        private readonly IWorkEventAggregator _workEventAggregator;

        public WorkItemQueueStore(IWorkEventAggregator workEventAggregator)
        {
            this._workEventAggregator = workEventAggregator;
        }

        public IWorkItemQueue<TE> GetOrCreate<TE>(QueueKind queueKind)
        {
            lock (_locker)
            {
                var item = _store.FirstOrDefault(x => x.Type == typeof(TE) && x.QueueKind == queueKind);
                if(item == null)
                {
                    item = new StoreItem { Type = typeof(TE), QueueKind = queueKind, Queue = new WorkItemQueue<TE>(_workEventAggregator, queueKind) };
                    _store.Add(item);
                }

                return item.Queue as IWorkItemQueue<TE>;
            }
        }


        private class StoreItem
        {
            public Type Type { get; set; }
            public QueueKind QueueKind { get; set; }

            public object Queue { get; set; }

        }
    }
}
