using Pi.Replicate.Queues;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public abstract class Worker<Tin>
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public Worker(TimeSpan waitTimeBetweenCycles)
        {
            WorkLoadInputQueue = WorkLoadQueueCache.CreateIfNotExist<Tin>();
            WaitTimeBetweenCycles = waitTimeBetweenCycles;
            CancellationToken = _cts.Token;
        }

        protected TimeSpan WaitTimeBetweenCycles { get; }
        protected CancellationToken CancellationToken { get; }
        protected BlockingCollection<Tin> WorkLoadInputQueue { get; set; }

        public virtual Task WorkAsync()
        {
            return Task.Run(() =>
            {
                while (!CancellationToken.IsCancellationRequested)
                {
                    DoWork();
                    CancellationToken.ThrowIfCancellationRequested();
                    Task.Delay(WaitTimeBetweenCycles);
                }
            });
        }

        public void StopWork()
        {
            _cts.Cancel();
        }

        protected abstract void DoWork();
    }

    public abstract class Worker<Tin, Tout> : Worker<Tin>
    {
        public Worker(TimeSpan waitTimeBetweenCycles, CancellationToken cancellationToken) : base(waitTimeBetweenCycles)
        {
            WorkLoadOutputQueue = WorkLoadQueueCache.CreateIfNotExist<Tout>();
        }

        protected BlockingCollection<Tout> WorkLoadOutputQueue { get; set; }


        public override Task WorkAsync()
        {
            return Task.Run(() =>
            {
                while (!CancellationToken.IsCancellationRequested && !WorkLoadInputQueue.IsCompleted)
                {
                    DoWork();
                    CancellationToken.ThrowIfCancellationRequested();
                    //not necessay to wait here. In this scenario the thread will wait as long as there are no item in the blockingcollection. Take blocks the thread
                    //Task.Delay(WaitTimeBetweenCycles);
                }
            });
        }
    }
}
