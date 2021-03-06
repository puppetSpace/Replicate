﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileDisassemblerWorker : BackgroundService
	{
		private readonly int _amountOfConcurrentJobs;
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly FileDisassemblerServiceFactory _fileProcessServiceFactory;
		private readonly TransmissionService _transmissionService;
		private readonly IRecipientRepository _recipientRepository;
		private readonly IWebhookService _webhookService;
		private readonly IFileRepository _fileRepository;

		public FileDisassemblerWorker(IConfiguration configuration, WorkerQueueContainer workerQueueContainer
			, FileDisassemblerServiceFactory fileProcessServiceFactory
			, TransmissionService transmissionService
			, IRecipientRepository recipientRepository
			, IWebhookService webhookService
			, IFileRepository fileRepository)
		{
			_amountOfConcurrentJobs = int.Parse(configuration[Constants.ConcurrentFileDisassemblyJobs]);
			_workerQueueContainer = workerQueueContainer;
			_fileProcessServiceFactory = fileProcessServiceFactory;
			_transmissionService = transmissionService;
			_recipientRepository = recipientRepository;
			_webhookService = webhookService;
			_fileRepository = fileRepository;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(Host.BackgroundWorkers.FileDisassemblerWorker)}");
				var incomingQueue = _workerQueueContainer.ToProcessFiles.Reader;
				var outgoingQueue = _workerQueueContainer.ToSendChunks.Writer;
				var taskRunner = new TaskRunner(_amountOfConcurrentJobs);
				while (await incomingQueue.WaitToReadAsync() && !stoppingToken.IsCancellationRequested)
				{
					var file = await incomingQueue.ReadAsync(stoppingToken);
					if (System.IO.File.Exists(file.GetFullPath()))
						taskRunner.Add(() => DissasemblyJob(outgoingQueue, file));
					else
						WorkerLog.Instance.Information($"File '{file.Path}' does not exist");
				}
			});

			th.Start();

			await Task.Delay(Timeout.Infinite);
		}

		private async Task DissasemblyJob(ChannelWriter<(Recipient recipient, FileChunk filechunk)> outgoingQueue, File file)
		{
			WorkerLog.Instance.Information($"'{file.Path}' is being processed");
			var recipients = await GetRecipients(file);

			if (recipients.Any())
			{
				var eofMessage = await _fileProcessServiceFactory.Get(file, new ChunkWriter(recipients, outgoingQueue)).ProcessFile();
				if (eofMessage is object)
				{
					_webhookService.NotifyFileDisassembled(file);
					foreach (var recipient in recipients)
						await _transmissionService.SendEofMessage(recipient, eofMessage);
				}
				else
				{
					_webhookService.NotifyFileFailed(file);
				}

				WorkerLog.Instance.Information($"'{file.Path}' is processed");
			}
			else
			{
				WorkerLog.Instance.Information($"No recipients found for folder containing '{file.Path}'");
			}
		}

		private async Task<List<Recipient>> GetRecipients(File file)
		{
			var recipients = new List<Recipient>();
			if (file is RequestFile rf)
				recipients = rf.Recipients.ToList();
			else
			{
				var recipientsResult = await _recipientRepository.GetRecipientsForFolder(file.FolderId);
				if (recipientsResult.WasSuccessful)
					recipients = recipientsResult.Data.ToList();
			}

			return recipients;
		}




	}
}
