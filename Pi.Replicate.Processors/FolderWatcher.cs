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
namespace Pi.Replicate.Processors
{
    public class FolderWatcher
    {
        private readonly Folder _folder;
        private readonly IRepositoryFactory _repository;
        private readonly IConfiguration _configuration;
        private static readonly ILogger _logger = LoggerFactory.Get<FolderWatcher>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public FolderWatcher(Folder folder, IRepositoryFactory repository, IConfiguration configuration)
        {
            _folder = folder;
            _repository = repository;
            _configuration = configuration;
        }

        public Task Watch()
        {
            return Task.Run(async () =>
            {
                if (_folder == null || !System.IO.Directory.Exists(_folder.GetPath()))
                    return;

                var fileObservable = new FileObservableBuilder(_repository)
                                    .Build(_folder);

                while (!_tokenSource.IsCancellationRequested)
                {
                    fileObservable.ProcessFiles();

                    var waitTime = _configuration["FolderWatcherPollDelay"] ?? "00:05:00";
                    await Task.Delay(TimeSpan.Parse(waitTime));
                }
            });
        }
    }
}
