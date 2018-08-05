using Microsoft.Extensions.Configuration;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared.Logging;
using Pi.Replicate.Shared.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public class FolderWatcher
    {
        private readonly Folder _folder;
        private readonly IRepository _repository;
        private readonly IFileSystem _fileSystem;
        private readonly IConfiguration _configuration;
        private static readonly ILogger _logger = LoggerFactory.Get<FolderWatcher>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public FolderWatcher(Folder folder, IRepository repository, IFileSystem fileSystem, IConfiguration configuration)
        {
            _folder = folder;
            _repository = repository;
            _fileSystem = fileSystem;
            _configuration = configuration;
        }

        public Task Watch()
        {
            return Task.Run(async () =>
            {
                if (_folder == null || !_fileSystem.DoesDirectoryExist(_folder.GetPath()))
                    return;

                while (!_tokenSource.IsCancellationRequested)
                {
                    var fileCollector = new FileCollector(_folder, _repository);
                    //fileCollector.Subscribe();
                    fileCollector.ProcessFiles();
                    var waitTime = _configuration["FolderWatcherPollDelay"] ?? "00:05:00";
                    await Task.Delay(TimeSpan.Parse(waitTime));
                }
            });
        }
    }
}
