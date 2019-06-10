using Pi.Replicate.Processing;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
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
		private readonly IRepository _repository;

		public Application(IWorkerFactory workerFactory, IRepository repository)
		{
			_workerFactory = workerFactory;
			_repository = repository;
		}

		public async Task Run()
		{
			//todo delete new files from database on startup.
			await _repository.FileRepository.DeleteNewFiles();

			var worker = _workerFactory.CreateProducerWorker(QueueKind.Outgoing);
			await worker.WorkAsync();

			var Folderconsumer = _workerFactory.CreateConsumerWorker(typeof(Folder), QueueKind.Outgoing);
			await Folderconsumer.WorkAsync();

			var fileConsumer = _workerFactory.CreateConsumerWorker(typeof(File), QueueKind.Outgoing);
			await fileConsumer.WorkAsync();
		}
    }
}
