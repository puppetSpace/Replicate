using Pi.Replicate.Processing;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Pi.Replicate.Queueing
{
    internal sealed class WorkItemQueue<TE> : ConcurrentQueue<TE>, IWorkItemQueue<TE>
    {
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
            if (item != null)
                base.Enqueue(item);

            return Task.CompletedTask;
        }

        public bool HasItems()
        {
            return base.Count != 0;
        }
    }
}
