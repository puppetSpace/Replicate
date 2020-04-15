using MediatR;
using Pi.Replicate.Application.Files.Queries.GetFilesForFolder;
using Pi.Replicate.Application.Folders.Processing;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Files.Processing
{
    public sealed class FileCollector
    {
        private readonly PathBuilder _pathBuilder;
        private readonly IMediator _mediator;
        private readonly Folder _folder;

        public FileCollector(PathBuilder pathBuilder, IMediator mediator, Folder folder)
        {
            _pathBuilder = pathBuilder;
            _mediator = mediator;
            _folder = folder;
        }

        public async Task<List<System.IO.FileInfo>> GetNewFiles()
        {
            (var rawFiles, var previousFilesInFolder) = await GetFiles();
            //exclude all files that are allready in the database
            var newFiles = rawFiles.Where(x => !previousFilesInFolder.Any(y => string.Equals(_pathBuilder.BuildPath(y.Path), x.FullName))).ToList();

            Log.Verbose($"{newFiles.Count} new files found in folder '{_folder.Name}'");

            return newFiles;
        }

        public async Task<List<System.IO.FileInfo>> GetChangedFiles()
        {
            (var rawFiles, var previousFilesInFolder) = await GetFiles();
            //only include the files that are already processed
            var changed = rawFiles
                    .Where(x => previousFilesInFolder
                        .Any(y => y.Status == FileStatus.Handled && x.FullName == _pathBuilder.BuildPath(y.Path) && x.LastWriteTimeUtc != y.LastModifiedDate))
                    .ToList();

            Log.Verbose($"{changed.Count} changed files found in folder '{_folder.Name}'");

            return changed;
        }

        private async Task<(IList<System.IO.FileInfo> AllFiles, List<File> ProcessedFiles)> GetFiles()
        {
            var folderPath = _pathBuilder.BuildPath(_folder.Name);
            if (_folder is null || !System.IO.Directory.Exists(folderPath))
            {
                Log.Warning($"Unable to get files. Given Folder path is null or does not exists. Value:'{folderPath}'.");
                throw new InvalidOperationException($"Unable to get files. Given Folder path is null or does not exists. Value:'{folderPath}'.");
            }

            var previousFilesInFolder = new List<File>();
            if (!_folder.FolderOptions.DeleteAfterSent)
            {
                previousFilesInFolder = await _mediator.Send(new GetFilesForFolderQuery(_folder.Id));
                Log.Verbose($"{previousFilesInFolder.Count} files already processed for folder '{_folder.Name}'.");
            }

            var folderCrawler = new FolderCrawler();
            folderCrawler.GetFiles(folderPath);

            return (folderCrawler.GetFiles(folderPath), previousFilesInFolder);
        }

    }

    public class FileCollectorFactory
    {
        private readonly PathBuilder _pathBuilder;
        private readonly IMediator _mediator;

        public FileCollectorFactory(PathBuilder pathBuilder, IMediator mediator)
        {
            _pathBuilder = pathBuilder;
            _mediator = mediator;
        }

        public FileCollector Get(Folder folder)
        {
            return new FileCollector(_pathBuilder, _mediator, folder);
        }
    }
}
