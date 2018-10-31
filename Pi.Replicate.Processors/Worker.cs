﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

#if DEBUG
[assembly: InternalsVisibleTo("Pi.Replicate.Test")]
#endif

namespace Pi.Replicate.Processing
{
    internal abstract class Worker
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

    internal abstract class Worker<Tout> : Worker
    {
        private readonly IWorkItemQueue<Tout> _outQueue;

        public Worker(IWorkItemQueueFactory workItemQueueFactory, QueueKind queueType)
        {
            _outQueue = workItemQueueFactory.GetQueue<Tout>(queueType);
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

    internal abstract class Worker<Tin, Tout> : Worker
    {
        private readonly IWorkItemQueue<Tin> _inQueue;
        private readonly IWorkItemQueue<Tout> _outQueue;

        public Worker(IWorkItemQueueFactory workItemQueueFactory, QueueKind queueType)
        {
            _inQueue = workItemQueueFactory.GetQueue<Tin>(queueType);
            _outQueue = workItemQueueFactory.GetQueue<Tout>(queueType);
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
