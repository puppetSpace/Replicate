using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System.Threading.Tasks;


namespace Pi.Replicate.Processing.Folders
{
    internal sealed class FolderWatcher : ProduceWorker<Folder>
    {
        private static readonly ILogger _logger = LoggerFactory.Get<FolderWatcher>();
        private readonly IRepository _repository;

        public FolderWatcher(IWorkItemQueueFactory workItemQueueFactory,IRepository repository)
            : base(workItemQueueFactory, QueueKind.Outgoing)
        {
            _repository = repository;
        }

        protected override async Task DoWork()
        {
            var folders = await _repository.FolderRepository.Get();
            foreach (var folder in folders)
            {
                await AddItem(folder);
            }
        }
    }
}
