using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Commands.MarkFileAsFailed;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Common;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileDisassemblerWorker : BackgroundService
	{
		private readonly int _amountOfConcurrentJobs;
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly FileDisassemblerService _fileProcessService;
		private readonly IMediator _mediator;
		private readonly TransmissionService _transmissionService;
		private readonly WebhookService _webhookService;

		public FileDisassemblerWorker(IConfiguration configuration, WorkerQueueContainer workerQueueContainer
			, FileDisassemblerService fileProcessService, IMediator mediator
			, TransmissionService transmissionService, WebhookService webhookService)
		{
			_amountOfConcurrentJobs = int.Parse(configuration[Constants.ConcurrentFileDisassemblyJobs]);
			_workerQueueContainer = workerQueueContainer;
			_fileProcessService = fileProcessService;
			_mediator = mediator;
			_transmissionService = transmissionService;
			_webhookService = webhookService;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				Log.Information($"Starting {nameof(Host.BackgroundWorkers.FileDisassemblerWorker)}");
				var incomingQueue = _workerQueueContainer.ToProcessFiles.Reader;
				var outgoingQueue = _workerQueueContainer.ToSendChunks.Writer;
				var taskRunner = new TaskRunner((int)_amountOfConcurrentJobs);
				while (await incomingQueue.WaitToReadAsync() && !stoppingToken.IsCancellationRequested)
				{
					var file = await incomingQueue.ReadAsync(stoppingToken);
					taskRunner.Add((async () =>
					{
						Log.Information($"'{file.Path}' is being processed");
						var recipients = await GetRecipients(file);

						if (recipients.Any())
						{
							var eofMessage = await SplitFile(file, recipients, outgoingQueue);
							if (eofMessage is object)
							{
								await FinializeFileProcess(eofMessage, recipients);
								_webhookService.NotifyFileDisassembled(file);
							}
							else
							{
								await _mediator.Send(new MarkFileAsFailedCommand { FileId = file.Id });
								_webhookService.NotifyFileFailed(file);
							}

							Log.Information($"'{file.Path}' is processed");
						}
					}));

				}
			});

			th.Start();

			return Task.CompletedTask;
		}

		private async Task<List<Recipient>> GetRecipients(File file)
		{
			var recipients = new List<Recipient>();
			if (file is RequestFile rf)
				recipients = rf.Recipients.ToList();
			else
			{
				var recipientsResult = await _mediator.Send(new GetRecipientsForFolderQuery { FolderId = file.FolderId });
				if (recipientsResult.WasSuccessful)
					recipients = recipientsResult.Data.ToList();
			}

			return recipients;
		}

		private async Task<EofMessage> SplitFile(File file, ICollection<Recipient> recipients, System.Threading.Channels.ChannelWriter<(Recipient recipient, FileChunk filechunk)> writer)
		{
			async Task ChunkCreatedCallBack(FileChunk fileChunk)
			{
				foreach (var recipient in recipients)
				{
					if (await writer.WaitToWriteAsync())
						await writer.WriteAsync((recipient, fileChunk));
				}
			}

			return await _fileProcessService.ProcessFile(file, ChunkCreatedCallBack);
		}

		private async Task FinializeFileProcess(EofMessage eofMessage, ICollection<Recipient> recipients)
		{
			foreach (var recipient in recipients)
				await _transmissionService.SendEofMessage(eofMessage, recipient);
		}


	}
}
