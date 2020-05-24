using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.FailedTransmissions.Commands.DeleteFailedTransmission;
using Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedEofMessageTransmissions;
using Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileChunkTransmissions;
using Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileTransmissions;
using Pi.Replicate.Application.Files.Queries.GetFailedFiles;
using Pi.Replicate.Application.Files.Queries.GetSignatureOfFile;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class RetryWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly IMediator _mediator;
		private readonly TransmissionService _transmissionService;
		private readonly WorkerQueueContainer _workerQueueContainer;

		public RetryWorker(IConfiguration configuration, IMediator mediator, TransmissionService transmissionService, WorkerQueueContainer workerQueueContainer)
		{
			_triggerInterval = int.TryParse(configuration[Constants.RetryTriggerInterval], out var interval) ? interval : 10;
			_mediator = mediator;
			_transmissionService = transmissionService;
			_workerQueueContainer = workerQueueContainer;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(RetryWorker)}");
				await RetryFailedFiles();
				await RetryFailedTransmisionFiles();
				await RetryFailedTransmissionEofMessages();
				await RetryFailedTransmissionFileChunks();

				Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min to trigger retry logic again");
				await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
			});
			th.Start();
			return Task.CompletedTask;
		}

		private async Task RetryFailedFiles()
		{
			Log.Information($"Adding failed file back on queue");
			var failedFilesResult = await _mediator.Send(new GetFailedFilesQuery());
			if (failedFilesResult.WasSuccessful)
			{
				var queue = _workerQueueContainer.ToProcessFiles.Writer;
				foreach (var failedFile in failedFilesResult.Data)
				{
					if(await queue.WaitToWriteAsync())
						await queue.WriteAsync(failedFile);
				}
			}
		}

		private async Task RetryFailedTransmisionFiles()
		{
			Log.Information($"Retrying to send files that have failed");
			var failedFilesResult = await _mediator.Send(new GetFailedFileTransmissionsForRetryQuery());
			if (failedFilesResult.WasSuccessful)
			{
				foreach (var ff in failedFilesResult.Data)
				{
					var signatureResult = await _mediator.Send(new GetSignatureOfFileQuery { FileId = ff.File.Id });
					var wasSucessful = await _transmissionService.SendFile(ff.Folder, ff.File, signatureResult.Data, ff.Recipient);
					if (wasSucessful)
						await _mediator.Send(new DeleteFailedFileTransmissionCommand { FileId = ff.File.Id, RecipientId = ff.Recipient.Id });
				}
			}
		}

		private async Task RetryFailedTransmissionEofMessages()
		{
			Log.Information($"Retrying to send eof messages that have failed");
			var failedEofMessagesResult = await _mediator.Send(new GetFailedEofMessageTransmissionsForRetryQuery());
			if (failedEofMessagesResult.WasSuccessful)
			{
				foreach (var fem in failedEofMessagesResult.Data)
				{
					var wasSuccessful = await _transmissionService.SendEofMessage(fem.EofMessage, fem.Recipient);
					if (wasSuccessful)
						await _mediator.Send(new DeleteFailedEofMessageTransmissionCommand { EofMessageId = fem.EofMessage.Id, RecipientId = fem.Recipient.Id });
				}
			}
		}

		private async Task RetryFailedTransmissionFileChunks()
		{
			Log.Information($"Retrying to send filechunks that have failed");
			var failedChunksResult = await _mediator.Send(new GetFailedFileChunkTransmissionsForRetryQuery());
			if (failedChunksResult.WasSuccessful)
			{
				foreach (var fc in failedChunksResult.Data)
				{
					var wasSuccessful = await _transmissionService.SendFileChunk(fc.FileChunk, fc.Recipient);
					if (wasSuccessful)
						await _mediator.Send(new DeleteFailedFileChunkTransmissionCommand { FileChunkId = fc.FileChunk.Id, RecipientId = fc.Recipient.Id });
				}
			}
		}


	}
}
