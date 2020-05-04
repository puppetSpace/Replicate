﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.EofMessages.Queries.GetFailedTransmissions;
using Pi.Replicate.Application.FileChunks.Queries.GetFailedTransmissions;
using Pi.Replicate.Application.Files.Queries.GetFailedTransmissions;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await RetryFailedFiles();
			await RetryFailedEofMessages();
			await RetryFailedChunks();

			Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min to trigger retry logic again");
			await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
		}

		private async Task RetryFailedFiles()
        {
            Log.Information($"Retrying to send files that have failed");
            var failedFiles = await _mediator.Send(new GetFailedFileTransmissionsForRetryCommand());
            foreach (var ff in failedFiles)
                await _transmissionService.SendFile(ff.Folder, ff.File, ff.Recipient);
        }

		private async Task RetryFailedEofMessages()
		{
			Log.Information($"Retrying to send eof messages that have failed");
			var failedEofMessages = await _mediator.Send(new GetFailedEofMessageTransmissionsForRetryCommand());
			foreach (var fem in failedEofMessages)
				await _transmissionService.SendEofMessage(fem.EofMessage, fem.Recipient);
		}

		private async Task RetryFailedChunks()
        {
			Log.Information($"Retrying to send filechunks that have failed");
			var failedChunks = await _mediator.Send(new GetFailedFileChunkTransmissionsForRetryCommand());
			foreach(var fc in failedChunks)
				await _transmissionService.SendFileChunk(fc.FileChunk,fc.Recipient);
		}


	}
}