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

		public List<System.IO.FileInfo> NewFiles { get; private set; }
		public List<System.IO.FileInfo> ChangedFiles { get; private set; }

		public async Task CollectFiles()
		{
			(var rawFiles, var previousFilesInFolder) = await GetFiles();
			//exclude all files that are allready in the database
			NewFiles = rawFiles.Where(x => !previousFilesInFolder.Any(y => string.Equals(_pathBuilder.BuildPath(y.Path), x.FullName))).ToList();

			Log.Information($"{NewFiles.Count} new file(s) found in folder '{_folder.Name}'");

			ChangedFiles = rawFiles
					.Where(x => previousFilesInFolder
						.Any(y => y.Status == FileStatus.Handled && x.FullName == _pathBuilder.BuildPath(y.Path) && x.LastWriteTimeUtc != y.LastModifiedDate))
					.ToList();

			Log.Information($"{ChangedFiles.Count} changed files found in folder '{_folder.Name}'");
		}

		private async Task<(IList<System.IO.FileInfo> AllFiles, ICollection<File> ProcessedFiles)> GetFiles()
		{
			var folderPath = _pathBuilder.BuildPath(_folder.Name);
			if (_folder is null || !System.IO.Directory.Exists(folderPath))
			{
				Log.Warning($"Unable to get files. Given Folder path is null or does not exists. Value:'{folderPath}'.");
				throw new InvalidOperationException($"Unable to get files. Given Folder path is null or does not exists. Value:'{folderPath}'.");
			}

			var previousFilesInFolder = await _mediator.Send(new GetFilesForFolderQuery(_folder.Id));
			Log.Information($"{previousFilesInFolder.Count} files already processed for folder '{_folder.Name}'.");

			var folderCrawler = new FolderCrawler();
			var files = folderCrawler.GetFiles(folderPath);

			return (files, previousFilesInFolder);
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
