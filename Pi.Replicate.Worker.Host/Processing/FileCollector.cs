using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Processing
{
	public sealed class FileCollector
	{
		private readonly PathBuilder _pathBuilder;
		private readonly IFileRepository _fileRepository;
		private readonly CrawledFolder _folder;

		public FileCollector(PathBuilder pathBuilder, IFileRepository fileRepository, CrawledFolder folder)
		{
			_pathBuilder = pathBuilder;
			_fileRepository = fileRepository;
			_folder = folder;
		}

		public List<System.IO.FileInfo> NewFiles { get; private set; } = new List<System.IO.FileInfo>();
		public List<System.IO.FileInfo> ChangedFiles { get; private set; } = new List<System.IO.FileInfo>();

		public async Task CollectFiles()
		{
			var filesFromSystem = GetFilesFromSystem();
			var result = await _fileRepository.GetFilesForFolder(_folder.Id);
			if (result.WasSuccessful)
			{
				var filesInDb = result.Data;
				WorkerLog.Instance.Information($"{filesInDb?.Count} file(s) already processed for folder '{_folder.Name}'.");

				NewFiles = filesFromSystem.Where(x => !filesInDb.Any(y => string.Equals(_pathBuilder.BuildPath(y.Path), x.FullName))).ToList();
				WorkerLog.Instance.Information($"{NewFiles.Count} new file(s) found in folder '{_folder.Name}'");

				ChangedFiles = filesFromSystem
						.Where(x => filesInDb
							.Any(y => x.FullName == _pathBuilder.BuildPath(y.Path) && x.LastWriteTimeUtc.TruncateMilliseconds() != y.LastModifiedDate.TruncateMilliseconds()))
						.ToList();

				WorkerLog.Instance.Information($"{ChangedFiles.Count} changed file(s) found in folder '{_folder.Name}'");
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
		private readonly IFileRepository _fileRepository;

		public FileCollectorFactory(PathBuilder pathBuilder, IFileRepository fileRepository)
		{
			_pathBuilder = pathBuilder;
			_fileRepository = fileRepository;
		}

		public FileCollector Get(CrawledFolder folder)
		{
			return new FileCollector(_pathBuilder, _fileRepository, folder);
		}
	}
}
