using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Queues
{
    public enum WorkerQueueType
    {
        ToProcessFiles = 0
    }

    public class WorkerQueueFactory
    {
        private readonly ConcurrentDictionary<WorkerQueueType, IDisposable> _queues = new ConcurrentDictionary<WorkerQueueType, IDisposable>();


        public BlockingCollection<TE> Get<TE>(WorkerQueueType workerQueueType)
        {
            var workerQueueTypeName = Enum.GetName(typeof(WorkerQueueType), workerQueueType);
            if (_queues.ContainsKey(workerQueueType))
            {
                Log.Verbose($"Getting existing queue for type {workerQueueTypeName}");
                if (_queues.TryGetValue(workerQueueType, out var queue))
                    return queue as BlockingCollection<TE>;
                else
                    throw new InvalidOperationException($"Unable to get Queue for {workerQueueTypeName}");
            }
            else
            {
                Log.Verbose($"Creating new queue for type {workerQueueTypeName}");
                var workerQueue = new BlockingCollection<TE>();
                _queues.AddOrUpdate(workerQueueType, workerQueue, (x, y) => workerQueue);
                return workerQueue;
            }
        }
    }
}
