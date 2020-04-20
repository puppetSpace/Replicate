using MediatR;
using Pi.Replicate.Application.Chunks.Commands.DeleteChunkPackage;
using Pi.Replicate.Application.Common.Queues;
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
		private readonly IMediator _mediator;
		private readonly IHttpClientFactory _httpClientFactory;

		public ChunkExportWorker(WorkerQueueFactory workerQueueFactory, IMediator mediator, IHttpClientFactory httpClientFactory)
		{
			_workerQueueFactory = workerQueueFactory;
			_mediator = mediator;
			_httpClientFactory = httpClientFactory;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var chunkPackage = queue.Take();
					var client = _httpClientFactory.CreateClient("default");

					Log.Verbose($"Sending chunk '{chunkPackage.FileChunk.SequenceNo}' of file with Id '{chunkPackage.FileChunk.FileId}' to '{chunkPackage.Recipient.Name}'");
					try
					{
						await client.PostAsync($"{chunkPackage.Recipient.Address}/Api/Chunk", chunkPackage.FileChunk, throwErrorOnResponseNok: true);
						await _mediator.Send(new DeleteChunkPackageCommand { RecipientId = chunkPackage.Recipient.Id, FileChunkId = chunkPackage.FileChunk.Id });
					}
					catch (Exception ex) when (ex is InvalidOperationException || ex is HttpRequestException)
					{
						Log.Error(ex, "Failed to send chunk");
					}
				}

			});

			thread.Start();
			return thread;
		}
	}
}
