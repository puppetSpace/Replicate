using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.FileChunks
{
    internal sealed class FileChunkDownload : Worker<FileChunk, object>
    {
        private readonly IRepository _repository;
        private readonly IUploadLink _uploadLink;

        public FileChunkDownload(IWorkItemQueueFactory workItemQueueFactory, Repositories.IRepository repository, IUploadLink uploadLink)
          : base(workItemQueueFactory, QueueKind.Incoming)
        {
            _repository = repository;
            _uploadLink = uploadLink;
        }

        protected async override Task DoWork(FileChunk workItem)
        {
            workItem.Status = ChunkStatus.Received;
            await _repository.FileChunkRepository.Save(workItem);
        }
    }
}
