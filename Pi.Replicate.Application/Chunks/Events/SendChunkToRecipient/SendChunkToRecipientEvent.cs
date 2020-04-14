using MediatR;
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

namespace Pi.Replicate.Application.Chunks.Events.SendChunkToRecipient
{
    public class SendChunkToRecipientEvent : IRequest
    {
        public ChunkPackage ChunkPackage { get; set; }
    }

    public class SendChunkToRecipientEventHandler : IRequestHandler<SendChunkToRecipientEvent>
    {
        private readonly HttpHelper _httpHelper;
        private readonly IWorkerContext _workerContext;

        public SendChunkToRecipientEventHandler(IHttpClientFactory httpClientFactory, IWorkerContext workerContext)
        {
            _httpHelper = new HttpHelper(httpClientFactory);
            _workerContext = workerContext;
        }

        public async Task<Unit> Handle(SendChunkToRecipientEvent request, CancellationToken cancellationToken)
        {
            try
            {
                await _httpHelper.Post($"{request.ChunkPackage.Recipient.Address}/Api/Chunk", request.ChunkPackage.FileChunk);
                _workerContext.ChunkPackages.Remove(request.ChunkPackage);
                await _workerContext.SaveChangesAsync(cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                Log.Error(ex, $"Failed to send chunk of file '{request.ChunkPackage.FileChunk.File.Path}' to '{request.ChunkPackage.Recipient.Name}'");
            }

            return Unit.Value;
        }
    }
}
