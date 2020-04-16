using MediatR;
using Pi.Replicate.Application.Chunks.Commands.DeleteChunkPackage;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System.Net.Http;
using System.Threading;

namespace Pi.Replicate.Workers
{
	public class ChunkExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;
		private readonly HttpHelper _httpHelper;

		public ChunkExportWorker(WorkerQueueFactory workerQueueFactory, IMediator mediator, IHttpClientFactory httpClientFactory)
		{
			_workerQueueFactory = workerQueueFactory;
			_mediator = mediator;
			_httpHelper = new HttpHelper(httpClientFactory);
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var chunkPackage = queue.Take();

					Log.Verbose($"Sending chunk '{chunkPackage.FileChunk.SequenceNo}' of file with Id '{chunkPackage.FileChunk.FileId}' to '{chunkPackage.Recipient.Name}'");
					await _httpHelper.Post($"{chunkPackage.Recipient.Address}/Api/Chunk", chunkPackage.FileChunk);
					await _mediator.Send(new DeleteChunkPackageCommand { ChunkPackageId = chunkPackage.Id });
				}

			});

			thread.Start();
			return thread;
		}
	}
}
