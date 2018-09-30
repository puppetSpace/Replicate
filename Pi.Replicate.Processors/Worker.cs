using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public abstract class Worker
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public Worker()
        {
            CancellationToken = _cts.Token;
        }

        protected CancellationToken CancellationToken { get; }

        public abstract Task WorkAsync();

        public virtual void Stop()
        {
            _cts.Cancel();
        }
    }

    public abstract class Worker<Tout> : Worker
    {
        private readonly TimeSpan _waitTimeBetweenCycles;
        private readonly IWorkItemQueue<Tout> _outQueue;

        public Worker(TimeSpan waitTimeBetweenCycles, IWorkItemQueueFactory workItemQueueFactory)
        {
            _outQueue = workItemQueueFactory.GetQueue<Tout>();
            _waitTimeBetweenCycles = waitTimeBetweenCycles;
        }

        public override Task WorkAsync()
        {
            return Task.Run(async () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    await DoWork();
                    CancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(_waitTimeBetweenCycles);
                }
            });
        }

        protected async Task AddItem(Tout workItem)
        {
            await _outQueue.Enqueue(workItem);
        }

        protected abstract Task DoWork();
    }

    public abstract class Worker<Tin, Tout> : Worker
    {
        private readonly IWorkItemQueue<Tin> _inQueue;
        private readonly IWorkItemQueue<Tout> _outQueue;
        private readonly TimeSpan _waitTimeBetweenCycles;

        public Worker(TimeSpan waitTimeBetweenCycles, IWorkItemQueueFactory workItemQueueFactory)
        {
            _inQueue = workItemQueueFactory.GetQueue<Tin>();
            _outQueue = workItemQueueFactory.GetQueue<Tout>();
            _waitTimeBetweenCycles = waitTimeBetweenCycles;
        }

        public override Task WorkAsync()
        {
            return Task.Run(async () =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    var value = await _inQueue.Dequeue();
                    await DoWork(value);
                    CancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(_waitTimeBetweenCycles);
                }
            });
        }

        protected async Task AddItem(Tout workItem)
        {
            await _outQueue.Enqueue(workItem);
        }

        protected abstract Task DoWork(Tin workItem);
    }
}
