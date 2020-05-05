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

		public List<System.IO.FileInfo> NewFiles { get; private set; } = new List<System.IO.FileInfo>();
		public List<System.IO.FileInfo> ChangedFiles { get; private set; } = new List<System.IO.FileInfo>();

		public async Task CollectFiles()
		{
			var filesFromSystem=  GetFilesFromSystem();
			var result = await _mediator.Send(new GetFilesForFolderQuery(_folder.Id));
			if (result.WasSuccessful)
			{
				var filesInDb = result.Data;
				Log.Information($"{filesInDb?.Count} file(s) already processed for folder '{_folder.Name}'.");

				NewFiles = filesFromSystem.Where(x => !filesInDb.Any(y => string.Equals(_pathBuilder.BuildPath(y.Path), x.FullName))).ToList();
				Log.Information($"{NewFiles.Count} new file(s) found in folder '{_folder.Name}'");

				ChangedFiles = filesFromSystem
						.Where(x => filesInDb
							.Any(y => x.FullName == _pathBuilder.BuildPath(y.Path) && x.LastWriteTimeUtc.TruncateMilliseconds() != y.LastModifiedDate.TruncateMilliseconds()))
						.ToList();

				Log.Information($"{ChangedFiles.Count} changed file(s) found in folder '{_folder.Name}'");
			}
		}

		private IList<System.IO.FileInfo> GetFilesFromSystem()
		{
			var folderPath = _pathBuilder.BuildPath(_folder.Name);
			var folderCrawler = new FolderCrawler();
			var files = folderCrawler.GetFiles(folderPath);

			return files;
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
