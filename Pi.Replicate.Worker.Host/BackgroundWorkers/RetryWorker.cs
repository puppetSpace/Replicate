using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class RetryWorker : BackgroundService
	{
		private readonly int _triggerInterval;
		private readonly TransmissionService _transmissionService;
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly IFileRepository _fileRepository;
		private readonly TransmissionRepository _transmissionRepository;

		public RetryWorker(IConfiguration configuration
			, TransmissionService transmissionService
			, WorkerQueueContainer workerQueueContainer
			, IFileRepository fileRepository
			, TransmissionRepository transmissionRepository
			)
		{
			_triggerInterval = int.TryParse(configuration[Constants.RetryTriggerInterval], out var interval) ? interval : 10;
			_transmissionService = transmissionService;
			_workerQueueContainer = workerQueueContainer;
			_fileRepository = fileRepository;
			_transmissionRepository = transmissionRepository;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
			await Task.Delay(Timeout.Infinite);
		}

		private async Task RetryFailedFiles()
		{
			Log.Information($"Adding failed file back on queue");
			var failedFilesResult = await _fileRepository.GetFailedFiles();
			if (failedFilesResult.WasSuccessful)
			{
				var queue = _workerQueueContainer.ToProcessFiles.Writer;
				foreach (var failedFile in failedFilesResult.Data)
				{
					if (await queue.WaitToWriteAsync())
						await queue.WriteAsync(failedFile);
				}
			}
		}

		private async Task RetryFailedTransmisionFiles()
		{
			Log.Information($"Retrying to send files that have failed");
			var failedFilesResult = await _transmissionRepository.GetFailedFileTransmission();
			if (failedFilesResult.WasSuccessful)
			{
				foreach (var ff in failedFilesResult.Data)
				{
					var wasSucessful = await _transmissionService.SendFile(ff.Item2, ff.Item1, ff.Item3);
					if (wasSucessful)
						await _transmissionRepository.DeleteFailedFileTransmission(ff.Item1.Id, ff.Item3.Id);
				}
			}
		}

		private async Task RetryFailedTransmissionEofMessages()
		{
			Log.Information($"Retrying to send eof messages that have failed");
			var failedEofMessagesResult = await _transmissionRepository.GetFailedEofMessageTransmission();
			if (failedEofMessagesResult.WasSuccessful)
			{
				foreach (var fem in failedEofMessagesResult.Data)
				{
					var wasSuccessful = await _transmissionService.SendEofMessage(fem.Item1, fem.Item2);
					if (wasSuccessful)
						await _transmissionRepository.DeleteFailedEofMessageTransmission(fem.Item1.Id, fem.Item2.Id);
				}
			}
		}

		private async Task RetryFailedTransmissionFileChunks()
		{
			Log.Information($"Retrying to send filechunks that have failed");
			var failedChunksResult = await _transmissionRepository.GetFailedFileChunkTransmission();
			if (failedChunksResult.WasSuccessful)
			{
				foreach (var fc in failedChunksResult.Data)
				{
					var wasSuccessful = await _transmissionService.SendFileChunk(fc.Item1, fc.Item2);
					if (wasSuccessful)
						await _transmissionRepository.DeleteFailedFileChunkTransmission(fc.Item1.Id, fc.Item2.Id);
				}
			}
		}


	}
}
