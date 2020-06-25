using Microsoft.Extensions.Hosting;
using Observr;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Processing;
using Pi.Replicate.Worker.Host.Repositories;
using Pi.Replicate.Worker.Host.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.BackgroundWorkers
{
	//todo retry mechanisme in database
	public class FileExportWorker : BackgroundService, Observr.IObserver<RecipientsAddedToFolderNotification>
	{
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly FolderRepository _folderRepository;
		private readonly IFileRepository _fileRepository;
		private readonly TransmissionService _communicationService;
		private readonly IDisposable _recipientsAddedNotificationSubscription;

		public FileExportWorker(FolderRepository folderRepository
			, IFileRepository fileRepository
			, TransmissionService communicationService
			, WorkerQueueContainer workerQueueContainer
			, IBroker broker)
		{
			_folderRepository = folderRepository;
			_fileRepository = fileRepository;
			_communicationService = communicationService;
			_workerQueueContainer = workerQueueContainer;
			_recipientsAddedNotificationSubscription = broker.Subscribe(this);
		}

		public async Task Handle(RecipientsAddedToFolderNotification value, CancellationToken cancellationToken)
		{
			WorkerLog.Instance.Information("Handling notification that recipients are added");
			WorkerLog.Instance.Debug($"Getting files of folder '{value.FolderId}' for following recipients: '{string.Join(", ", value.Recipients)}'");
			var filesResult = await _fileRepository.GetFilesForFolder(value.FolderId);
			var folderResult = await _folderRepository.GetFolder(value.FolderId);
			if (filesResult.WasSuccessful && folderResult.WasSuccessful)
			{
				var neededRecipients = folderResult.Data.Recipients?.Where(x => value.Recipients.Any(y => y == x.Id)).ToList();
				foreach (var file in filesResult.Data)
				{
					var requestFile = new RequestFile
					{
						Id = file.Id,
						FolderId = file.FolderId,
						LastModifiedDate = file.LastModifiedDate,
						Name = file.Name,
						Path = file.Path,
						Size = file.Size,
						Source = file.Source,
						Version = file.Version,
						Recipients = neededRecipients
					};

					await _workerQueueContainer.ToSendFiles.Writer.WriteAsync(requestFile, cancellationToken);
				}
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var th = new Thread(async () =>
			{
				WorkerLog.Instance.Information($"Starting {nameof(FileExportWorker)}");
				var incomingQueue = _workerQueueContainer.ToSendFiles.Reader;
				var outgoingQueue = _workerQueueContainer.ToProcessFiles.Writer;
				while (await incomingQueue.WaitToReadAsync() || !stoppingToken.IsCancellationRequested)
				{
					var file = await incomingQueue.ReadAsync();
					var folderResult = await _folderRepository.GetFolder(file.FolderId);

					if (folderResult.WasSuccessful)
					{
						var recipients = file is RequestFile rf ? rf.Recipients : folderResult.Data.Recipients;
						foreach (var recipient in recipients)
							await _communicationService.SendFile(folderResult.Data, file, recipient);

						if (await outgoingQueue.WaitToWriteAsync())
							await outgoingQueue.WriteAsync(file);
					}
				}
			});

			th.Start();

			await Task.Delay(Timeout.Infinite);
			_recipientsAddedNotificationSubscription.Dispose();
		}
	}
}