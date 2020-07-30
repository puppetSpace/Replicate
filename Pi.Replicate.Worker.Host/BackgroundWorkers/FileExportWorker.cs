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
	public class FileExportWorker : BackgroundService, Observr.IObserver<RecipientsAddedToFolderNotification>
	{
		private readonly WorkerQueueContainer _workerQueueContainer;
		private readonly IFolderRepository _folderRepository;
		private readonly IFileRepository _fileRepository;
		private readonly TransmissionService _communicationService;
		private readonly IDisposable _recipientsAddedNotificationSubscription;

		public FileExportWorker(IFolderRepository folderRepository
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
					(
						file.Id,
						file.FolderId,
						file.Name,
						file.RelativePath,
						file.LastModifiedDate,
						file.Size,
						file.Source,
						file.Version,
						neededRecipients
					);

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
							await _communicationService.SendFile(recipient, folderResult.Data, file);

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