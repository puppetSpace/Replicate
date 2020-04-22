using MediatR;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Application.Common.Queues;
using Pi.Replicate.Application.Files.Commands.AddNewFile;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Application.Files.Queries.GetFileForChange;
using Pi.Replicate.Application.Folders.Queries.GetFoldersToCrawl;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    //todo on start delete files with status new and changed. When theyhave this status, it means the process got shut down before chunks were made
    public class FolderWorker : WorkerBase
    {
        private readonly int _triggerInterval;
        private readonly IMediator _mediator;
        private readonly FileCollectorFactory _fileCollectorFactory;
        private readonly WorkerQueueFactory _workerQueueFactory;
        private readonly PathBuilder _pathBuilder;

        public FolderWorker(IConfiguration configuration
            , IMediator mediator
            , FileCollectorFactory fileCollectorFactory
            , WorkerQueueFactory workerQueueFactory
            , PathBuilder pathBuilder)
        {
            _triggerInterval = int.TryParse(configuration[Constants.FolderCrawlTriggerInterval], out int interval) ? interval : 10;
            _mediator = mediator;
            _fileCollectorFactory = fileCollectorFactory;
            _workerQueueFactory = workerQueueFactory;
            _pathBuilder = pathBuilder;
        }

        public override Thread DoWork(CancellationToken cancellationToken)
        {
            var workingThread = new Thread(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var folders = await _mediator.Send(new GetFoldersToCrawlQuery(), cancellationToken);
                    foreach (var folder in folders)
                    {
                        Log.Information($"Crawling through folder '{folder.Name}'");
                        var collector = _fileCollectorFactory.Get(folder);
                        await collector.CollectFiles();
                        await ProcessNewFiles(folder, collector.NewFiles);
                        await ProcessChangedFiles(collector.ChangedFiles);
                    }

                    Log.Information($"Waiting {TimeSpan.FromMinutes(_triggerInterval)}min for next cycle of foldercrawling");
                    await Task.Delay(TimeSpan.FromMinutes(_triggerInterval));
                }

            });
            workingThread.Start();
            return workingThread;
        }

        private async Task ProcessNewFiles(Folder folder, List<System.IO.FileInfo> newFiles)
        {
            var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
            foreach (var newFile in newFiles)
            {
                var file = File.BuildPartial(newFile, folder.Id, _pathBuilder.BasePath);
                await _mediator.Send(new AddNewFileCommand { File = file });
                Log.Debug($"Adding '{file.Path}' to queue");
                if (queue.Any(x => string.Equals(x.Path, file.Path)))
                    Log.Information($"{file.Path} already present in queue for processing");
                else
                    queue.Add(file);
            }
        }

        private async Task ProcessChangedFiles(List<System.IO.FileInfo> changedFiles)
        {
            //can't save changes to file here. If creating delta fails, I can't rollback to previous change
            var queue = _workerQueueFactory.Get<File>(WorkerQueueType.ToProcessFiles);
            foreach (var file in changedFiles)
            {
                var relativePath = file.FullName.Replace(_pathBuilder.BasePath + "\\", "");
                var foundFile = await _mediator.Send(new GetFileForChangeQuery { Path = relativePath });
                if (foundFile is object)
                {
                    foundFile.UpdateForChange(file);
                    Log.Debug($"Adding '{foundFile.Path}' to queue");
                    if (queue.Any(x => string.Equals(x.Path, foundFile.Path)))
                        Log.Information($"{foundFile.Path} already present in queue for processing");
                    else
                        queue.Add(foundFile);
                }
                else
                {
                    Log.Information($"Unable to perform update action. No file found with path '{relativePath}'");
                }
            }
        }
    }
}
