using Microsoft.Extensions.Configuration;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using Pi.Replicate.Shared.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#if DEBUG
[assembly: InternalsVisibleTo("Pi.Replicate.Test")]
#endif
namespace Pi.Replicate.Processors.Folders
{
    public class FolderWatcher : Worker<Folder>
    {
        private readonly IFolderRepository _repository;
        private readonly IConfiguration _configuration;
        private static readonly ILogger _logger = LoggerFactory.Get<FolderWatcher>();

        public FolderWatcher(IRepositoryFactory repository, IConfiguration configuration)
            :base(TimeSpan.Parse(configuration["FolderWatcherPollDelay"] ?? "00:05:00"))
        {
            _repository = repository.CreateFolderRepository();
            _configuration = configuration;
        }

        protected override void DoWork()
        {
            var folders = _repository.Get();
            foreach(var folder in folders)
            {
                WorkLoadInputQueue.Add(folder);
            }
        }
    }
}
