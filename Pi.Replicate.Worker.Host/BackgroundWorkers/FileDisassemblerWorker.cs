using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Common;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

//todo add Conflicts: Double versions of file, version missing of file, ...
namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	public class FileDisassemblerWorker : BackgroundService
	{
		private readonly int _amountOfConcurrentJobs;
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly FileDisassemblerService _fileProcessService;
		private readonly TransmissionService _transmissionService;
		private readonly RecipientRepository _recipientRepository;
		private readonly PathBuilder _pathBuilder;

		public FileDisassemblerWorker(IConfiguration configuration, WorkerQueueContainer workerQueueContainer
			, FileDisassemblerService fileProcessService
			, TransmissionService transmissionService
			, RecipientRepository recipientRepository
			, PathBuilder pathBuilder)
		{
			_amountOfConcurrentJobs = int.Parse(configuration[Constants.ConcurrentFileDisassemblyJobs]);
			_workerQueueContainer = workerQueueContainer;
			_fileProcessService = fileProcessService;
			_transmissionService = transmissionService;
			_recipientRepository = recipientRepository;
			_pathBuilder = pathBuilder;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(FileDisassemblerWorker)}");
				var incomingQueue = _workerQueueContainer.ToProcessFiles.Reader;
				var outgoingQueue = _workerQueueContainer.ToSendChunks.Writer;
				var taskRunner = new TaskRunner(_amountOfConcurrentJobs);
				while (await incomingQueue.WaitToReadAsync() && !stoppingToken.IsCancellationRequested)
				{
					var file = await incomingQueue.ReadAsync(stoppingToken);
					if (System.IO.File.Exists(_pathBuilder.BuildPath(file.Path)))
					{
						taskRunner.Add(async () =>
						{
							WorkerLog.Instance.Information($"'{file.Path}' is being processed");
							var recipients = await GetRecipients(file);

							if (recipients.Any())
							{
								var eofMessage = await SplitFile(file, recipients, outgoingQueue);
								if (eofMessage is object)
									await FinializeFileProcess(eofMessage, recipients);

								WorkerLog.Instance.Information($"'{file.Path}' is processed");
							}
						});
					}
					else
					{
						WorkerLog.Instance.Information($"File '{file.Path}' does not exist");
					}
				}
			});

			th.Start();

			await Task.Delay(Timeout.Infinite);
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
