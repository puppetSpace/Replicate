using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Events.SendFileToRecipient
{
    public class SendFileToRecipientEvent : IRequest
    {
        public File File { get; set; }

        public Recipient Recipient { get; set; }
    }

    public class SendFileToRecipientEventHandler : IRequestHandler<SendFileToRecipientEvent>
    {
		private readonly WorkerQueueFactory _workerQueueFactory;
		private readonly IWorkerContext _workerContext;
		private readonly HttpHelper _httpHelper;

		public SendFileToRecipientEventHandler(IMediator mediator, IHttpClientFactory httpClientFactory, WorkerQueueFactory workerQueueFactory, IWorkerContext workerContext)
		{
			_workerQueueFactory = workerQueueFactory;
			_workerContext = workerContext;
			_httpHelper = new HttpHelper(httpClientFactory);
		}

        public async Task<Unit> Handle(SendFileToRecipientEvent request, CancellationToken cancellationToken)
        {
			var sucess = await SendFile(request.File, request.Recipient,cancellationToken);

			if (sucess)
			{
				var outgoingQueue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
				var packages = CreateChunkPackages(request.File.Id, request.Recipient, cancellationToken);
				await foreach (var package in packages)
					outgoingQueue.Add(package);

				request.File.MarkAsHandled();
				_workerContext.Files.Update(request.File);

				await _workerContext.SaveChangesAsync(cancellationToken);
			}

			return Unit.Value;
		}

		private async IAsyncEnumerable<ChunkPackage> CreateChunkPackages(Guid fileId, Recipient recipient, CancellationToken cancellationToken)
		{

			var chunks = _workerContext
				.FileChunks
				.Include(x => x.File)
				.AsNoTracking()
				.Where(x => x.FileId == fileId);

			foreach (var chunk in chunks)
			{
				var package = ChunkPackage.Build(chunk, recipient);
				_workerContext.ChunkPackages.Add(package);
				yield return package;
			}

			await _workerContext.SaveChangesAsync(cancellationToken);
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
				_workerContext.FailedFiles.Add(FailedFile.Build(file.Id, recipient.Id));
				await _workerContext.SaveChangesAsync(cancellationToken);
				return false;
			}
		}
	}
}
