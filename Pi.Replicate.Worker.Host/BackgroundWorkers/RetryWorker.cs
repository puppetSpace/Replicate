using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedEofMessageTransmissions;
using Pi.Replicate.Application.FailedTransmissions.Commands.AddFailedTransmission;
using Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileChunkTransmissions;
using Pi.Replicate.Application.Files.Queries.GetSignatureOfFile;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;
using Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileTransmissions;
using Pi.Replicate.Application.FailedTransmissions.Commands.DeleteFailedTransmission;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class RetryWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly IMediator _mediator;
		private readonly TransmissionService _transmissionService;

		public RetryWorker(IConfiguration configuration, IMediator mediator, TransmissionService transmissionService)
		{
			_triggerInterval = int.TryParse(configuration[Constants.RetryTriggerInterval], out var interval) ? interval : 10;
			_mediator = mediator;
			_transmissionService = transmissionService;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				await RetryFailedFiles();
				await RetryFailedEofMessages();
				await RetryFailedChunks();

				Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min to trigger retry logic again");
				await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
			});
			th.Start();
			return Task.CompletedTask;
		}

		private async Task RetryFailedFiles()
		{
			Log.Information($"Retrying to send files that have failed");
			var failedFilesResult = await _mediator.Send(new GetFailedFileTransmissionsForRetryQuery());
			if (failedFilesResult.WasSuccessful)
			{
				foreach (var ff in failedFilesResult.Data)
				{
					var signatureResult = await _mediator.Send(new GetSignatureOfFileQuery { FileId = ff.File.Id });
					if (signatureResult.WasSuccessful)
					{
						var wasSucessful = await _transmissionService.SendFile(ff.Folder, ff.File, signatureResult.Data, ff.Recipient);
						if(wasSucessful)
							await _mediator.Send(new DeleteFailedFileTransmissionCommand { FileId = ff.File.Id, RecipientId = ff.Recipient.Id });
					}
					else
					{
						await _mediator.Send(new AddFailedFileTransmissionCommand { FileId = ff.File.Id, RecipientId = ff.Recipient.Id });
					}
				}
			}
		}

		private async Task RetryFailedEofMessages()
		{
			Log.Information($"Retrying to send eof messages that have failed");
			var failedEofMessagesResult = await _mediator.Send(new GetFailedEofMessageTransmissionsForRetryQuery());
			if (failedEofMessagesResult.WasSuccessful)
			{
				foreach (var fem in failedEofMessagesResult.Data)
				{
					var wasSuccessful = await _transmissionService.SendEofMessage(fem.EofMessage, fem.Recipient);
					if(wasSuccessful)
						await _mediator.Send(new DeleteFailedEofMessageTransmissionCommand { EofMessageId = fem.EofMessage.Id, RecipientId = fem.Recipient.Id });
				}
			}
		}

		private async Task RetryFailedChunks()
		{
			Log.Information($"Retrying to send filechunks that have failed");
			var failedChunksResult = await _mediator.Send(new GetFailedFileChunkTransmissionsForRetryQuery());
			if (failedChunksResult.WasSuccessful)
			{
				foreach (var fc in failedChunksResult.Data)
				{
					var wasSuccessful = await _transmissionService.SendFileChunk(fc.FileChunk, fc.Recipient);
					if(wasSuccessful)
						await _mediator.Send(new DeleteFailedFileChunkTransmissionCommand { FileChunkId = fc.FileChunk.Id, RecipientId = fc.Recipient.Id });
				}
			}
		}


	}
}
