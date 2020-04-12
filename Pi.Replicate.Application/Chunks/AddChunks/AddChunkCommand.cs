using MediatR;
using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Chunks.AddChunks
{
    public class AddChunkCommand : IRequest
    {
        public byte[] Chunk { get; set; }

        public int SequenceNo { get; set; }

        public byte[] Hash { get; set; }

        public File File { get; set; }

        public ChunkSource ChunkSource { get; set; }
    }

    public class AddChunkCommandHandler : IRequestHandler<AddChunkCommand>
    {
        private readonly IWorkerContext _workerContext;
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly PathBuilder _pathBuilder;

        public AddChunkCommandHandler(IWorkerContext workerContext, WorkerQueueFactory workerQueueFactory, PathBuilder pathBuilder)
        {
            _workerContext = workerContext;
            _workerQueueFactory = workerQueueFactory;
            _pathBuilder = pathBuilder;
        }

        //transform each byte[] to a chunk and add it to the database.
        //for each recipient of the folder from where the file is, create a chunkpackage and add it to the database and queue to send
        public async Task<Unit> Handle(AddChunkCommand request, CancellationToken cancellationToken)
        {
            var queue = _workerQueueFactory.Get<ChunkPackage>(WorkerQueueType.ToSendChunks);
            var builtChunk = FileChunk.Build(request.File, request.SequenceNo, request.Chunk, ChunkSource.FromNewFile);
            _workerContext.FileChunks.Add(builtChunk);

            foreach (var recipient in request.File.Folder.Recipients)
            {
                var chunkPackage = ChunkPackage.Build(builtChunk.Id, recipient.RecipientId);
                _workerContext.ChunkPackages.Add(chunkPackage);
                queue.Add(chunkPackage);
            }

            // request.File.UpdateAfterProcessesing(request.Chunks.Count, request.Hash);
            // 	_workerContext.Files.Update(request.File);

            // await _workerContext.SaveChangesAsync(cancellationToken);


            // if (request.File.Folder.FolderOptions.DeleteAfterSent)
            // {
            //     var path = _pathBuilder.BuildPath(request.File);
            //     System.IO.File.Delete(path);
            // }

            return Unit.Value;
        }
    }
}
