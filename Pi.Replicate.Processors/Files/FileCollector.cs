﻿using Microsoft.Extensions.Configuration;
using Pi.Replicate.Processors.Folders;
using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Processors.Repositories;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Files
{
    internal class FileCollector : Worker<Folder,File>
    {
        private readonly List<IObserver<File>> _observers = new List<IObserver<File>>();
        private static readonly ILogger _logger = LoggerFactory.Get<FileCollector>();
        private readonly IRepository _repository;

        public FileCollector(IRepository repository, IWorkItemQueueFactory workItemQueueFactory)
            : base(workItemQueueFactory, QueueKind.Outgoing)
        {
            _repository = repository;
        }

        protected async override Task DoWork(Folder folder)
        {
            var files = new List<Schema.File>();
            if (folder == null || !System.IO.Directory.Exists(folder.GetPath()))
            {
                _logger.Warn($"Unable to get files. Given Folder path is null or does not exists. Value:'{folder?.GetPath()}'.");
                throw new InvalidOperationException($"Unable to get files. Given Folder path is null or does not exists. Value:'{folder?.GetPath()}'.");
            }
            var newOrChanged = await GetNewOrChanged(folder);

            await SaveAndSplitFiles(folder, newOrChanged);
        }

        private async Task<IList<string>> GetNewOrChanged(Folder folder)
        {
            var previousFilesInFolder = new List<File>();
            if (!folder.DeleteFilesAfterSend)
            {
                var files = await _repository.FileRepository.GetSent(folder.Id);
                previousFilesInFolder = files.Where(x => x.Status == FileStatus.Sent || x.Status == FileStatus.New).ToList(); //_folder.Files.Where(x => x.Status == FileStatus.Sent || x.Status == FileStatus.New).ToList();
                _logger.Trace($"{previousFilesInFolder.Count} files already processed for folder '{folder.GetPath()}'.");
            }
            var folderCrawler = new FolderCrawler();
            var rawFiles = folderCrawler.GetFiles(folder.GetPath());
            var newFiles = rawFiles.Except(previousFilesInFolder.Select(x => x.GetPath())).ToList();

            var changed = rawFiles
                .Select(x => new System.IO.FileInfo(x))
                    .Where(x => previousFilesInFolder
                        .Any(y => x.FullName == y.GetPath() && x.LastWriteTimeUtc != y.LastModifiedDate))
                    .Select(x => x.FullName)
                .ToList();

            _logger.Trace($"{newFiles.Count} new files found and {changed.Count} files were changed for folder '{folder.GetPath()}'");

            return newFiles.Union(changed).ToList();
        }

        private async Task SaveAndSplitFiles(Folder folder, IList<string> newOrChanged)
        {
            foreach (var file in newOrChanged)
            {
                var fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Exists)
                {
                    var fileObject = CreateFileObject(folder, fileInfo);
                    await _repository.FileRepository.Save(fileObject);
                    await AddItem(fileObject);
                }
            }
        }

        private File CreateFileObject(Folder folder, System.IO.FileInfo fileInfo)
        {
            return new Schema.File
            {
                Id = Guid.NewGuid(),
                Folder = folder,
                Name = fileInfo.Name,
                Size = fileInfo.Length,
                Status = FileStatus.New
            };
        }


    }
}