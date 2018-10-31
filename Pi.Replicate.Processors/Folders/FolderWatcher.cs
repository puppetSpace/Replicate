using Pi.Replicate.Processing.Repositories;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using System.Threading.Tasks;


namespace Pi.Replicate.Processing.Folders
{
    internal sealed class FolderWatcher : Worker<Folder>
    {
        private static readonly ILogger _logger = LoggerFactory.Get<FolderWatcher>();
        private readonly IRepository _repository;

        public FolderWatcher(IRepository repository, IWorkItemQueueFactory workItemQueueFactory)
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
