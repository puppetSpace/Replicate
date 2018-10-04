using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Files
{
    internal class FileAssembler : Worker<File, object> //no outgoing, only incoming
    {
        private readonly IFileChunkRepository _fileChunkRepository;
        private readonly IFileRepository _fileRepository;
        private readonly ILogger _logger = LoggerFactory.Get<FileAssembler>();

        public FileAssembler(IWorkItemQueueFactory workItemQueueFactory, IFileChunkRepository fileChunkRepository, IFileRepository fileRepository)
            : base(workItemQueueFactory, QueueKind.Incoming)
        {
            _fileChunkRepository = fileChunkRepository;
            _fileRepository = fileRepository;
        }

        protected override async Task DoWork(File file)
        {
            _logger.Info($"Assembling file '{file.Name}'");
            var receivedChunks = await _fileChunkRepository.Get(file.Id);
            _logger.Debug($"File '{file.Name}' has {receivedChunks.Count()} chunks of bytes");
            //todo check if folder already exist
            var rawBytes = AssembleChunks(receivedChunks);
            var tempFile = await WriteToTemp(rawBytes,file.Extension);
            var isSucessfull = CopyFileToDestination(tempFile, file);
            if (isSucessfull)
            {
                RemoveChunks(file.Id);
                SaveFileAsReceivedComplete(file);
            }
        }


        private IEnumerable<byte[]> AssembleChunks(IEnumerable<FileChunk> receivedChunks)
        {
            return receivedChunks
                .OrderBy(x => x.SequenceNo)
                .Select(x => Convert.FromBase64String(x.Value));

        }

        private async Task<string> WriteToTemp(IEnumerable<byte[]> rawBytes, string extension)
        {
            //todo uncompress if needed
            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName(), extension);
            var fileStream = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.Write); ;

            foreach (var chunk in rawBytes)
            {
                await fileStream.WriteAsync(chunk, 0, chunk.Length);
                await fileStream.FlushAsync();
            }
            await fileStream.FlushAsync();

            return tempFile;
        }

        private bool CopyFileToDestination(string tempFile, File file)
        {
            //todo delete temp file or save it so it can be used next time
            if (System.IO.File.Exists(file.GetPath()) && FileLock.IsLocked(file.GetPath()))
            {
                _logger.Warn($"File {file.GetPath()} exist and cannot be overriden because it is locked");
                return false;
            }
            _logger.Info($"Moving tempfile '{tempFile}' to destination '{file.GetPath()}'");
            System.IO.File.Move(tempFile, file.GetPath());
            return true;
        }

        private void RemoveChunks(Guid fileId)
        {
            _logger.Info($"Deleting the chunks of file with id {fileId}");
            _fileChunkRepository.Delete(fileId);
        }

        private void SaveFileAsReceivedComplete(File file)
        {
            _logger.Info($"Updating status of file {file.Name} in database");
            file.Status = FileStatus.ReceivedComplete;
            _fileRepository.Update(file);
        }
    }
}
