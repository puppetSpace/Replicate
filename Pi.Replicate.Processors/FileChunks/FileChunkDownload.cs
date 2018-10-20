using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Processors.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.FileChunks
{
    internal class FileChunkDownload : Worker<FileChunk, object>
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
            await _repository.FileChunkRepository.Save(workItem);
        }
    }
}
