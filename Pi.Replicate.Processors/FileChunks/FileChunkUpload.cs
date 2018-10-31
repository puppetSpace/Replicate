using Pi.Replicate.Processing.Communication;
using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.FileChunks
{
    internal sealed class FileChunkUpload : ConsumeWorker<FileChunk>
    {
        private readonly ILogger _logger = LoggerFactory.Get<FileChunkUpload>();
        private readonly IRepository _repository;
        private readonly IUploadLink _uploadLink;

        public FileChunkUpload(IWorkItemQueueFactory workItemQueueFactory, IRepository repository, IUploadLink uploadLink) 
            : base(workItemQueueFactory, QueueKind.Outgoing)
        {
            _repository = repository;
            _uploadLink = uploadLink;
        }

        protected async override Task DoWork(FileChunk workItem)
        {
            //todo check if FileChunk can be casted to ResendFileChunk so that that destination can be used
            _logger.Info($"Uploading chunk '{workItem.SequenceNo}' of file '{workItem.File?.Name}'");
            var hosts = await _repository.HostRepository.GetDestinationHosts(workItem.File.Folder.Id);
            _logger.Debug($"Folder '{workItem.File.Folder.Name}' has '{hosts.Count()}' host to send data to");

            foreach(var host in hosts)
            {
                var isSuccesfull = await SendData(host, workItem);
                if (!isSuccesfull)
                    await _repository.FileChunkRepository.SaveFailed(new FailedUploadFileChunk { FileChunk = workItem, Host = host });
            }
        }

        private async Task<bool> SendData(Host host, FileChunk workItem)
        {
            var response = await _uploadLink.UploadData(host.Address, workItem);
            _logger.Info($"Reponse from '{host.Address}': {(response.IsSuccessful ? "OK" : response.ErrorMessage)}");
            return response.IsSuccessful;
        }
    }
}
