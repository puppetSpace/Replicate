using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Agent.Send
{
    public class Application
    {
		private readonly IWorkerFactory _workerFactory;

		public Application(IWorkerFactory workerFactory)
		{
			_workerFactory = workerFactory;
		}

		public async Task Run()
		{
			var worker = _workerFactory.CreateProducerWorker(QueueKind.Outgoing);
			await worker.WorkAsync();
		}
    }
}
