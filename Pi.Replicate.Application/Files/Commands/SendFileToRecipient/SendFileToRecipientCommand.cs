using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Commands.SendFileToRecipient
{
	public class SendFileToRecipientCommand : IRequest
	{
		public File File { get; set; }

		public Recipient Recipient { get; set; }
	}

	public class SendFileToRecipientCommandHandler : IRequestHandler<SendFileToRecipientCommand>
	{
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IWorkerContext _workerContext;
		private readonly HttpHelper _httpHelper;

		public SendFileToRecipientCommandHandler(IMediator mediator, IHttpClientFactory httpClientFactory, WorkerQueueFactory workerQueueFactory, IWorkerContext workerContext)
		{
			_workerQueueFactory = workerQueueFactory;
			_workerContext = workerContext;
			_httpHelper = new HttpHelper(httpClientFactory);
		}

		public async Task<Unit> Handle(SendFileToRecipientCommand request, CancellationToken cancellationToken)
		{
			//todo Folder must also be created
			var sucess = await SendFile(request.File, request.Recipient, cancellationToken);

			if (sucess)
			{
				await CreateChunkPackages(request.File, request.Recipient);
				request.File.MarkAsHandled();
				await _workerContext.FileRepository.Update(request.File);
			}

			return Unit.Value;
		}

		private async Task CreateChunkPackages(File file, Recipient recipient)
		{
			var outgoingQueue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);

			int processedChunks = 0;
			while (processedChunks < file.AmountOfChunks)
			{
				var chunks = await _workerContext
					.FileChunkRepository
					.GetForFile(file.Id,processedChunks,100);

				processedChunks += chunks.Count;

				foreach (var chunk in chunks)
				{
					var package = ChunkPackage.Build(chunk, recipient);
					await _workerContext.ChunkPackageRepository.Create(package);
					outgoingQueue.Add(package);
				}
			}
		}

		private async Task<bool> SendFile(File file, Recipient recipient, CancellationToken cancellationToken)
		{
			try
			{
				var endpoint = $"{recipient.Address}/api/file";
				await _httpHelper.Post(endpoint, file);
				return true;
			}
			catch (System.InvalidOperationException ex)
			{
				Log.Error(ex, $"Failed to send file metadata of '{file.Path}' to '{recipient.Name}'. Adding file to FailedFiles and retrying later");
				await _workerContext.FailedFileRepository.Create(FailedFile.Build(file, recipient));
				return false;
			}
		}
	}
}
