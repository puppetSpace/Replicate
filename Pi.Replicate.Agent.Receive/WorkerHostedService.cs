using Microsoft.Extensions.Hosting;
using Pi.Replicate.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent.Api
{
    public class WorkerHostedService : IHostedService
    {
        private readonly WorkManager _workManager;

        public WorkerHostedService(WorkManager workManager)
        {
            _workManager = workManager;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //todo pass along cancellationtoken to workers

            _workManager.Start();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
