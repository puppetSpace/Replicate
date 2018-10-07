using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Processors.Repositories;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Files
{
    internal class FileAssembler : Worker<File,object>
    {
        private readonly IRepository _repository;
        private readonly IUploadLink _uploadLink;
        private readonly ILogger _logger = LoggerFactory.Get<FileAssembler>();

        public FileAssembler(IWorkItemQueueFactory workItemQueueFactory, IRepository repository , IUploadLink uploadLink)
            : base(workItemQueueFactory, QueueKind.Incoming)
        {
            _repository = repository;
            _uploadLink = uploadLink;
        }


        protected override async Task DoWork(File file)
        {
            _logger.Info($"Assembling file '{file.Name}'");
            var possibleTemp = await _repository.FileRepository.GetTempFileIfExists(file);
            var tempFilePath = string.Empty;

            if (!IsTempFileStillValid(possibleTemp, file))
            {
                DeletePossibleTempFile(possibleTemp);
                var receivedChunks = await _repository.FileChunkRepository.Get(file.Id);
                _logger.Debug($"File '{file.Name}' has {receivedChunks.Count()} chunks of bytes");
                var rawBytes = AssembleChunks(receivedChunks);
                tempFilePath = await WriteToTemp(rawBytes);
            }
            else
            {
                _logger.Info($"using earlier created temp file for '{file.Name}'");
                tempFilePath = possibleTemp.Path;
            }

            
            var isSucessfull = MoveFileToDestination(tempFilePath, file);
            if (isSucessfull)
            {
                await RemoveChunks(file.Id);
                await SaveFileAsReceivedComplete(file);
            }
        }
        
        private IEnumerable<byte[]> AssembleChunks(IEnumerable<FileChunk> receivedChunks)
        {
            return receivedChunks
                .OrderBy(x => x.SequenceNo)
                .Select(x => Convert.FromBase64String(x.Value));

        }

        private async Task<string> WriteToTemp(IEnumerable<byte[]> rawBytes)
        {
            //todo uncompress if needed
            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
            using (var fileStream = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {

                foreach (var chunk in rawBytes)
                {
                    await fileStream.WriteAsync(chunk, 0, chunk.Length);
                    await fileStream.FlushAsync();
                }
                await fileStream.FlushAsync();
            }

            return tempFile;
        }

        private bool MoveFileToDestination(string tempFile, File file)
        {
            if (System.IO.File.Exists(file.GetPath()) && FileLock.IsLocked(file.GetPath()))
            {
                _logger.Warn($"File {file.GetPath()} exist and cannot be overriden because it is locked");
                _repository.FileRepository.SaveTemp(new TempFile { FileId = file.Id, Hash = file.Hash, Path = tempFile });
                return false;
            }
            _logger.Info($"Moving tempfile '{tempFile}' to destination '{file.GetPath()}'");
            System.IO.File.Move(tempFile, file.GetPath());
            return true;
        }

        private async Task RemoveChunks(Guid fileId)
        {
            _logger.Info($"Deleting the chunks of file with id {fileId}");
            await _repository.FileChunkRepository.DeleteForFile(fileId);
        }

        private async Task SaveFileAsReceivedComplete(File file)
        {
            _logger.Info($"Updating status of file {file.Name} in database");
            file.Status = FileStatus.ReceivedComplete;
            await _repository.FileRepository.Update(file);
            await _repository.FileRepository.DeleteTempFile(file.Id);
            await _uploadLink.FileReceived(file.Source, file.Id);
        }

        private bool IsTempFileStillValid(TempFile possibletemp, File file)
        {
            return possibletemp != null
                && possibletemp.Hash == file.Hash
                && System.IO.File.Exists(possibletemp.Path);
        }

        private void DeletePossibleTempFile(TempFile possibleTemp)
        {
            if (possibleTemp != null && System.IO.File.Exists(possibleTemp.Path))
                System.IO.File.Delete(possibleTemp.Path);
        }


    }
}
