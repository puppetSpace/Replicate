using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Processors.Repositories;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Files
{
    internal class FileChecker : Worker<File>
    {
        private readonly IRepository _repository;
        private readonly IUploadLink _uploadLink;
        private readonly ILogger _logger = LoggerFactory.Get<FileChecker>();

        public FileChecker(IWorkItemQueueFactory workItemQueueFactory, IRepository repository, IUploadLink uploadLink)
            : base(workItemQueueFactory, QueueKind.Incoming)

        {
            _repository = repository;
            _uploadLink = uploadLink;
        }

        protected override async Task DoWork()
        {
            var files = await _repository.FileRepository.GetCompletedReceivedFiles();
            _logger.Info($"Checking hash of {files.Count()} received files");
            foreach (var file in files)
            {
                var hash = await CreateHashFromChunks(file);
                if (file.Hash == hash)
                {
                    _logger.Info($"Hash of file {file.Name} is good. Adding item to queue");
                    await AddItem(file);
                }
                else
                {
                    _logger.Info($"Hash of file {file.Name} is bad. Requesting a resend");
                    _logger.Debug($"Requesting resend of host {file.Source}");
                    await _repository.FileRepository.Delete(file.Id);
                    await _repository.FileChunkRepository.DeleteForFile(file.Id);
                    await _uploadLink.RequestResendOfFile(file.Source, file.Id);
                }
            }
        }

        private async Task<string> CreateHashFromChunks(File file)
        {
            var chunks = await _repository.FileChunkRepository.Get(file.Id);
            var rawBytes = chunks.OrderBy(x => x.SequenceNo)
                .SelectMany(x => System.Convert.FromBase64String(x.Value))
                .ToArray();

            var hashCreator = MD5.Create();
            var hash = System.Convert.ToBase64String(hashCreator.ComputeHash(rawBytes));
            return hash;
        }
    }
}
