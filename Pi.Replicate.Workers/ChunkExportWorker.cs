using MediatR;
using Pi.Replicate.Application.Chunks.Commands.DeleteChunkPackage;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Net.Http;
using System.Threading;

namespace Pi.Replicate.Workers
{
	public class ChunkExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly CommunicationService _communicationService;

		public ChunkExportWorker(WorkerQueueFactory workerQueueFactory, CommunicationService communicationService)
		{
			_workerQueueFactory = workerQueueFactory;
			_communicationService = communicationService;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var chunkPackage = queue.Take();
					await _communicationService.SendChunkPackage(chunkPackage);
				}

			});

			thread.Start();
			return thread;
		}
	}
}
