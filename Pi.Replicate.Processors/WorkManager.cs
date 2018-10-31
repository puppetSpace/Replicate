using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Pi.Replicate.Processing.Folders;
using Pi.Replicate.Processing.FileChunks;
using Pi.Replicate.Processing.Files;

namespace Pi.Replicate.Processing
{
    public sealed class WorkManager
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
        private Timer _timer;

        public WorkManager(IConfiguration configuration)
        {
            _pollDelay = TimeSpan.TryParse(configuration[Constants.PollDelay], out var pollDelay) ? pollDelay : TimeSpan.FromMinutes(10);
            _timer = new Timer(_pollDelay.TotalMilliseconds);
            _timer.AutoReset = true;
            _timer.Elapsed += TimerElapsed;
        }

        public void Start()
        {
            //todo notified executions for Consumers / Producer-Consumers

            _timer.Start();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if(!_activeWorkers.ContainsWorker<FolderWatcher>())
            {
                //todo inject dependencies
                var fw = new FolderWatcher(null, null);
                _activeWorkers.Add(new ActiveWorker(fw));
            }

            if (!_activeWorkers.ContainsWorker<FileChecker>())
            {
                //todo inject dependencies
                var fc = new FileChecker(null, null,null);
                _activeWorkers.Add(new ActiveWorker(fc));
            }
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

    internal sealed class ActiveWorkerCollection : ConcurrentDictionary<Guid,ActiveWorker>
    {
        public bool ContainsWorker<TE>() where TE : Worker
        {
            return Values.Any(x => x.Worker.GetType() == typeof(TE));
        }

        public bool Add(ActiveWorker activeWorker)
        {
            //todo action when failed
            if (base.TryAdd(activeWorker.Id, activeWorker))
            {
                activeWorker.Task.ContinueWith((t, s) => TryRemove((Guid)s, out var deletedItem), activeWorker.Id, TaskContinuationOptions.ExecuteSynchronously);
                return true;
            }

            return false;
        }

        public new bool TryAdd(Guid key, ActiveWorker value)
        {
            return Add(value);
        }

        public void Remove(Guid activeWorkerId)
        {
            //todo action when failed
            TryRemove(activeWorkerId, out var deletedItem);
        }
    }
}
