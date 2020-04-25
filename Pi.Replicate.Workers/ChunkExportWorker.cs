using MediatR;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace Pi.Replicate.Workers
{
	public class ChunkExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly TransmissionService _transmissionService;

		public ChunkExportWorker(WorkerQueueFactory workerQueueFactory, TransmissionService transmissionService)
		{
			_workerQueueFactory = workerQueueFactory;
			_transmissionService = transmissionService;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var queue = _workerQueueFactory.Get<KeyValuePair<Recipient, FileChunk>>(WorkerQueueType.ToSendChunks);
				while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var chunkPackage = queue.Take();
					await _transmissionService.SendFileChunk(chunkPackage.Value, chunkPackage.Key);
				}

			});

			thread.Start();
			return thread;
		}
	}
}
