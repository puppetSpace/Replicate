using System;
using System.Collections.Concurrent;

namespace Pi.Replicate.Queues
{
    public class WorkLoadQueueHandler<TE>
    {
        private BlockingCollection<TE> _queue = new BlockingCollection<TE>();

        public WorkLoadQueueHandler()
        {
            _queue = WorkLoadQueueCache.CreateIfNotExist<TE>();
        }

        public void Add(TE value)
        {
            _queue.Add(value);
        }

        public TE Get()
        {
            return _queue.Take();
        }
    }
}
