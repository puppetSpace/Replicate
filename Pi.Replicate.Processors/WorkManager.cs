using Microsoft.Extensions.Configuration;
using Pi.Replicate.Processing.Files;
using Pi.Replicate.Processing.Folders;
using Pi.Replicate.Processing.Notification;
using Pi.Replicate.Processing.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Pi.Replicate.Processing
{
    public sealed class WorkManager : IWorkSubscriber
    {
        //RECEIVING
        //FileAssembler (Consumer)
        //FileChecker (Producer)
        //FileChunkDownload (Consumer)


        //SENDING
        //FileCollector (Producer-Consumer)
        //FileSplitter (Producer-Consumer)
        //FolderWatcher (Producer)
        //FileChunkUpload (Consumer)

        //Flow
        // FolderWatcher -> FileCollector -> FileSplitter -> FileChunkUpload #TRANSMISSION# FileChunkDownload -> FileChecker -> FileAssembler
        //                       IN              IN                IN                              IN                                IN
        //                   FolderQueue      FileQueue      FileChunkQueue                   FileChunkQueue                      FileQueue
        //      OUT              OUT             OUT                                                                 OUT         
        //  FolderQueue       FileQueue      FileChunkQueue                                                       FileQueue


        //SENDING queues: FolderQueue,FileQueue & FileChunkQueue
        //RECEIVING queues: FileChunkQueue & FileQueue

        private readonly ActiveWorkerCollection _activeWorkers = new ActiveWorkerCollection();
        private readonly TimeSpan _pollDelay;
        private readonly IWorkerFactory _workerFactory;
        private const int _maxWorkloadForWorker = 100;
        private Timer _timer;

        public WorkManager(IConfiguration configuration, IWorkEventAggregator workEventAggregator, IWorkerFactory workerFactory)
        {
            _pollDelay = TimeSpan.TryParse(configuration[Constants.PollDelay], out var pollDelay) ? pollDelay : TimeSpan.FromMinutes(10);
            workEventAggregator.Subscribe(this);
            _timer = new Timer(_pollDelay.TotalMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += TimerElapsed;
            _workerFactory = workerFactory;
        }

        public void Start()
        {
            CreateProduceWorkers(); //already create before timer elapses
            _timer.Start();
        }

        public void WorkCreated(WorkEventData workEventData)
        {
            var amountOfWorkers = _activeWorkers.AmountOfConsumeWorkers(workEventData.TypeOfWorkData, workEventData.QueueKind);
            var maxAmountOfWorkload = _maxWorkloadForWorker * amountOfWorkers;

            if (workEventData.CurrentWorkload > maxAmountOfWorkload)
            {
                var workersToAdd = Math.Ceiling((double)((workEventData.CurrentWorkload - maxAmountOfWorkload) / _maxWorkloadForWorker));
                for(int aow = 0; aow < workersToAdd; aow++)
                    _activeWorkers.Add(new ActiveWorker(_workerFactory.CreateConsumerWorker(workEventData.TypeOfWorkData, workEventData.QueueKind)));
            }
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            CreateProduceWorkers();
        }

        private void CreateProduceWorkers()
        {
            if (_activeWorkers.AmountOfProduceWorkers(QueueKind.Outgoing) == 0)
                _activeWorkers.Add(new ActiveWorker(_workerFactory.CreateProducerWorker(QueueKind.Outgoing)));

            if (_activeWorkers.AmountOfProduceWorkers(QueueKind.Incoming) == 0)
                _activeWorkers.Add(new ActiveWorker(_workerFactory.CreateProducerWorker(QueueKind.Incoming)));
        }
    }

    internal sealed class ActiveWorker
    {

        public ActiveWorker(Worker worker)
        {
            Id = Guid.NewGuid();
            Worker = worker;
            Task = worker.WorkAsync();
        }

        public Worker Worker { get; private set; }

        public Task Task { get; private set; }

        public Guid Id { get; private set; }
    }

    internal sealed class ActiveWorkerCollection : List<ActiveWorker>
    {
        private readonly object _locker = new object();

        public int AmountOfProduceWorkers(QueueKind queueKind)
        {
            lock (_locker)
                return this.Count(x => x.Worker.ConsumeType == null && x.Worker.QueueKind == queueKind);
        }

        public int AmountOfConsumeWorkers(Type type, QueueKind queueKind)
        {
            lock (_locker)
                return this.Count(x => x.Worker.ConsumeType != null && (x.Worker.ConsumeType == type && x.Worker.QueueKind == queueKind));
        }

        public new void Add(ActiveWorker activeWorker)
        {
            //todo action when failed
            lock (_locker)
            {
                base.Add(activeWorker);
            }
            activeWorker.Task.ContinueWith((t, s) => base.Remove((ActiveWorker)s), activeWorker, TaskContinuationOptions.ExecuteSynchronously);
        }

    }
}
