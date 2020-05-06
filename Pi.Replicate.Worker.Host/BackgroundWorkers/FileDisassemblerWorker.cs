using MediatR;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Commands.MarkFileAsFailed;
using Pi.Replicate.Application.Recipients.Queries.GetRecipientsForFolder;
using Pi.Replicate.Application.Services;
using Pi.Replicate.Domain;
using Serilog;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileDisassemblerWorker : BackgroundService
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly FileDisassemblerService _fileProcessService;
		private readonly IMediator _mediator;
		private readonly TransmissionService _transmissionService;
		public FileDisassemblerWorker(WorkerQueueFactory workerQueueFactory, FileDisassemblerService fileProcessService, IMediator mediator, TransmissionService transmissionService)
		{
			_workerQueueFactory = workerQueueFactory;
			_fileProcessService = fileProcessService;
			_mediator = mediator;
			_transmissionService = transmissionService;
		}

		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(() =>
			{
				Log.Information($"Starting {nameof(FileDisassemblerWorker)}");
				var incomingQueue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
				var outgoingQueue = _workerQueueFactory.Get<KeyValuePair<Recipient, FileChunk>>(WorkerQueueType.ToSendChunks);
				var runningTasks = new List<Task>();
				var semaphore = new SemaphoreSlim(10); //todo create setting for this
				while (!incomingQueue.IsCompleted && !stoppingToken.IsCancellationRequested)
				{
					runningTasks.RemoveAll(x => x.IsCompleted);
					var file = incomingQueue.Take(stoppingToken);

					runningTasks.Add(Task.Run(async () =>
					{
						await semaphore.WaitAsync();
						Log.Information($"'{file.Path}' is being processed");
						var recipientsResult = await _mediator.Send(new GetRecipientsForFolderQuery { FolderId = file.FolderId });
						if (recipientsResult.WasSuccessful)
						{
							var eofMessage = await SplitFile(file, recipientsResult.Data, outgoingQueue);
							if (eofMessage is object)
								await FinializeFileProcess(eofMessage, recipientsResult.Data);
							else
								await _mediator.Send(new MarkFileAsFailedCommand { FileId = file.Id });

							Log.Information($"'{file.Path}' is processed");
						}
						semaphore.Release();
					}));

				}
			});

			th.Start();

			return Task.CompletedTask;
		}

		private async Task<EofMessage> SplitFile(File file, ICollection<Recipient> recipients, BlockingCollection<KeyValuePair<Recipient, FileChunk>> queue)
		{
			void ChunkCreatedCallBack(FileChunk fileChunk)
			{
				foreach (var recipient in recipients)
					queue.Add(new KeyValuePair<Recipient, FileChunk>(recipient, fileChunk));
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
