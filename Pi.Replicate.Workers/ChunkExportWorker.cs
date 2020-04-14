using MediatR;
using Pi.Replicate.Application.Chunks.Events.SendChunkToRecipient;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Serilog;
using System.Threading;

namespace Pi.Replicate.Workers
{
	public class ChunkExportWorker : WorkerBase
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IMediator _mediator;

		public ChunkExportWorker(WorkerQueueFactory workerQueueFactory, IMediator mediator)
		{
			_workerQueueFactory = workerQueueFactory;
			_mediator = mediator;
		}

		public override Thread DoWork(CancellationToken cancellationToken)
		{
			var thread = new Thread(async () =>
			{
				var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				while (!queue.IsCompleted || !cancellationToken.IsCancellationRequested)
				{
					var chunkPackage = queue.Take();

					Log.Verbose($"Sending chunk '{chunkPackage.FileChunk.SequenceNo}' of file '{chunkPackage.FileChunk.File.Path}' to '{chunkPackage.Recipient.Name}'");
					await _mediator.Send(new SendChunkToRecipientEvent { ChunkPackage = chunkPackage });
				}

			});

			thread.Start();
			return thread;
		}
	}
}
