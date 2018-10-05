using Pi.Replicate.Processors.Communication;
using Pi.Replicate.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Files
{
    public class FileChecker : Worker<File>
    {
        private readonly IFileChunkRepository _fileChunkRepository;
        private readonly IFileRepository _fileRepository;
        private readonly IUploadLink _uploadLink;

        public FileChecker(IWorkItemQueueFactory workItemQueueFactory, IFileChunkRepository fileChunkRepository, IFileRepository fileRepository, IUploadLink uploadLink)
            : base(workItemQueueFactory, QueueKind.Incoming)

        {
            _fileChunkRepository = fileChunkRepository;
            _fileRepository = fileRepository;
            _uploadLink = uploadLink;
        }

        //todo refactor
        protected override async Task DoWork()
        {
            var files = await _fileRepository.GetCompletedReceivedFiles();
            foreach (var file in files)
            {
                var chunks = await _fileChunkRepository.Get(file.Id);
                var rawBytes = chunks.OrderBy(x=>x.SequenceNo)
                    .SelectMany(x => System.Convert.FromBase64String(x.Value))
                    .ToArray();

                var hashCreator = MD5.Create();
                var hash = System.Convert.ToBase64String(hashCreator.ComputeHash(rawBytes));
                if(file.Hash == hash)
                {
                    await AddItem(file);
                }
                else
                {
                    await _fileRepository.Delete(file.Id);
                    await _fileChunkRepository.DeleteForFile(file.Id);
                    await _uploadLink.RequestResendOfFile(file.Source, file.Id);
                }
            }
        }
    }
}
