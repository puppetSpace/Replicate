using Pi.Replicate.Queues;
using System;
using System.Collections.Concurrent;
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

    public abstract class Worker<Tin> : Worker
    {
        private readonly BlockingCollection<Tin> _workLoadInputQueue;
        private readonly TimeSpan _waitTimeBetweenCycles;

        public Worker(TimeSpan waitTimeBetweenCycles)
        {
            _workLoadInputQueue = WorkLoadQueueCache.CreateIfNotExist<Tin>();
            _waitTimeBetweenCycles = waitTimeBetweenCycles;
        }

        public override Task WorkAsync()
        {
            return Task.Run(() =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    DoWork();
                    CancellationToken.ThrowIfCancellationRequested();
                    Task.Delay(_waitTimeBetweenCycles);
                }
            });
        }

        protected void AddItem(Tin workItem)
        {
            _workLoadInputQueue.TryAdd(workItem);
        }

        protected abstract void DoWork();
    }

    public abstract class Worker<Tin, Tout> : Worker
    {
        private readonly BlockingCollection<Tin> _workLoadInputQueue;
        private readonly BlockingCollection<Tout> _workLoadOutputQueue;

        public Worker()
        {
            _workLoadInputQueue = WorkLoadQueueCache.CreateIfNotExist<Tin>();
            _workLoadOutputQueue = WorkLoadQueueCache.CreateIfNotExist<Tout>();
        }

        public override Task WorkAsync()
        {
            return Task.Run(() =>
            {
                while (!CancellationToken.IsCancellationRequested && !_workLoadInputQueue.IsCompleted)
                {
                    if (_workLoadInputQueue.TryTake(out var value))
                    {
                        DoWork(value);
                    }
                    CancellationToken.ThrowIfCancellationRequested();
                }
            });
        }

        protected void AddItem(Tout workItem)
        {
            _workLoadOutputQueue.TryAdd(workItem);
        }

        protected abstract void DoWork(Tin workItem);
    }
}
