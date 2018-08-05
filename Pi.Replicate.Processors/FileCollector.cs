﻿using Pi.Replicate.Processors.Builders;
using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Schema;
using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public class FileCollector : Observable<File>
    {
        private readonly Folder _folder;
        private readonly IRepository _repository;
        private readonly List<IObserver<File>> _observers = new List<IObserver<File>>();
        private static readonly ILogger _logger = LoggerFactory.Get<FileCollector>();

        public FileCollector(Folder folder, IRepository repository)
        {
            _folder = folder;
            _repository = repository;
        }

        public void ProcessFiles()
        {
                var files = new List<Schema.File>();
                if (_folder == null || !System.IO.Directory.Exists(_folder.GetPath()))
                {
                    _logger.Info($"Unable to get files. Given Folder path is null or does not exists. Value:'{_folder?.GetPath()}'.");
                }
                var newOrChanged = GetNewOrChanged();

                SaveAndSplitFiles(newOrChanged);
        }

        private IList<string> GetNewOrChanged()
        {
            var previousFilesInFolder = new List<File>();
            if (!_folder.DeleteFilesAfterSend)
            {
                previousFilesInFolder = _repository.FileRepository.Get(_folder.Id).Where(x => x.Status == FileStatus.Sent || x.Status == FileStatus.New).ToList(); //_folder.Files.Where(x => x.Status == FileStatus.Sent || x.Status == FileStatus.New).ToList();
                _logger.Trace($"{previousFilesInFolder.Count} files already processed for folder '{_folder.GetPath()}'.");
            }
            var folderCrawler = new FolderCrawler();
            var rawFiles = folderCrawler.GetFiles(_folder.GetPath());
            var newFiles = rawFiles.Except(previousFilesInFolder.Select(x => x.GetPath())).ToList();

            var changed = rawFiles
                .Select(x => new System.IO.FileInfo(x))
                    .Where(x => previousFilesInFolder
                        .Any(y => x.FullName == y.GetPath() && x.LastWriteTimeUtc != y.LastModifiedDate))
                    .Select(x => x.FullName)
                .ToList();

            _logger.Trace($"{newFiles.Count} new files found and {changed.Count} files were changed for folder '{_folder.GetPath()}'");

            return newFiles.Union(changed).ToList();
        }

        private void SaveAndSplitFiles(IList<string> newOrChanged)
        {
            foreach (var file in newOrChanged)
            {
                var fileInfo = new System.IO.FileInfo(file);
                if (fileInfo.Exists)
                {
                    var fileObject = FileBuilder.Build(_folder, fileInfo);
                    Notify(fileObject);
                }
            }
            NotifyComplete();
        }
    }
}
