﻿using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileChunkExportWorker : BackgroundService
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly TransmissionService _transmissionService;

		public FileChunkExportWorker(WorkerQueueFactory workerQueueFactory, TransmissionService transmissionService)
		{
			_workerQueueFactory = workerQueueFactory;
			_transmissionService = transmissionService;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(FileChunkExportWorker)}");
				var queue = _workerQueueFactory.Get<KeyValuePair<Recipient, FileChunk>>(WorkerQueueType.ToSendChunks);
				while (!queue.IsCompleted || !stoppingToken.IsCancellationRequested)
				{
					var chunkPackage = queue.Take();
					await _transmissionService.SendFileChunk(chunkPackage.Value, chunkPackage.Key);
				}
			});
			th.Start();

			return Task.CompletedTask;
		}
	}
}