﻿using Serilog;
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
        ToProcessFiles = 0,
        ToSendFiles = 1,
        ToSendChunks = 2
    }

    public class WorkerQueueFactory
    {
        private readonly Dictionary<WorkerQueueType, IDisposable> _queues = new Dictionary<WorkerQueueType, IDisposable>();


        public BlockingCollection<TE> Get<TE>(WorkerQueueType workerQueueType)
        {
            lock (_queues) {
                var workerQueueTypeName = Enum.GetName(typeof(WorkerQueueType), workerQueueType);
                if (_queues.ContainsKey(workerQueueType))
                {
                    Log.Debug($"Getting existing queue for type {workerQueueTypeName}");
                    if (_queues.TryGetValue(workerQueueType, out var queue))
                        return queue as BlockingCollection<TE>;
                    else
                        throw new InvalidOperationException($"Unable to get Queue for {workerQueueTypeName}");
                }
                else
                {
                    Log.Debug($"Creating new queue for type {workerQueueTypeName}");
                    var workerQueue = new BlockingCollection<TE>();
                    _queues.Add(workerQueueType, workerQueue);
                    return workerQueue;
                }
            }
        }
    }
}
