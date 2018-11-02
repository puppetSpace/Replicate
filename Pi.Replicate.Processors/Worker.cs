using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#if DEBUG
[assembly: InternalsVisibleTo("Pi.Replicate.Test")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
#endif

namespace Pi.Replicate.Processing
{
    public abstract class Worker
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public Worker()
        {
            CancellationToken = _cts.Token;
        }

        public Type ConsumeType { get; protected set; }

        public QueueKind QueueKind { get; protected set; }

        protected CancellationToken CancellationToken { get; }

        public abstract Task WorkAsync();

        public virtual void Stop()
        {
            _cts.Cancel();
        }
    }

    internal abstract class ProduceWorker<Tout> : Worker
    {
        private readonly IWorkItemQueue<Tout> _outQueue;

        public ProduceWorker(IWorkItemQueueFactory workItemQueueFactory, QueueKind queueKind)
        {
            _outQueue = workItemQueueFactory.GetQueue<Tout>(queueKind);
            QueueKind = queueKind;
        }

        public override async Task WorkAsync()
        {
            await DoWork();
        }

        protected async Task AddItem(Tout workItem)
        {
            await _outQueue.Enqueue(workItem);
        }

        protected abstract Task DoWork();
    }

    internal abstract class ConsumeWorker<Tin> : Worker
    {
        private readonly IWorkItemQueue<Tin> _inQueue;

        public ConsumeWorker(IWorkItemQueueFactory workItemQueueFactory, QueueKind queueKind)
        {
            _inQueue = workItemQueueFactory.GetQueue<Tin>(queueKind);
            ConsumeType = typeof(Tin);
            QueueKind = queueKind;
        }

        public override async Task WorkAsync()
        {
            while (_inQueue.HasItems())
            {
                var value = await _inQueue.Dequeue();
                await DoWork(value);
                CancellationToken.ThrowIfCancellationRequested();
            }
        }

        protected abstract Task DoWork(Tin workItem);
    }

    internal abstract class ProduceConsumeWorker<Tin, Tout> : Worker
    {
        private readonly IWorkItemQueue<Tin> _inQueue;
        private readonly IWorkItemQueue<Tout> _outQueue;

        public ProduceConsumeWorker(IWorkItemQueueFactory workItemQueueFactory, QueueKind queueKind)
        {
            _inQueue = workItemQueueFactory.GetQueue<Tin>(queueKind);
            _outQueue = workItemQueueFactory.GetQueue<Tout>(queueKind);

            ConsumeType = typeof(Tin);
            QueueKind = queueKind;
        }

        public override async Task WorkAsync()
        {
            while (_inQueue.HasItems())
            {
                var value = await _inQueue.Dequeue();
                await DoWork(value);
                CancellationToken.ThrowIfCancellationRequested();
            }
        }

        protected async Task AddItem(Tout workItem)
        {
            await _outQueue.Enqueue(workItem);
        }

        protected abstract Task DoWork(Tin workItem);
    }
}
