using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Models;
using Pi.Replicate.Worker.Host.Repositories;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("Pi.Replicate.Test")]
namespace Pi.Replicate.Worker.Host.Services
{
	public class FileAssemblerService
	{
		private readonly IDatabase _database;
		private readonly IFileRepository _fileRepository;
		private readonly IFileChunkRepository _fileChunkRepository;


		public FileAssemblerService(IDatabase database
			, IFileRepository fileRepository
			, IFileChunkRepository fileChunkRepository)
		{
			_database = database;
			_fileRepository = fileRepository;
			_fileChunkRepository = fileChunkRepository;
		}

		public async Task<bool> ProcessFile(File file, EofMessage eofMessage)
		{
			try
			{
				//this class has alot of access to database. Just open 1 connection instead of every call creating its own
				using (_database)
				{
					if (file.IsNew())
						await ProcessNew(file, eofMessage);
					else
						await ProcessChange(file, eofMessage);
				}
				return true;
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Unexpected error occured while assembling the file '{file.Name}'");
				return false;
			}
		}

		private async Task ProcessNew(File file, EofMessage eofMessage)
		{
			var tempPath = await BuildFile(file, eofMessage);
			if (tempPath is null)
				return;

			var filePath = file.GetFullPath();

			if (System.IO.File.Exists(filePath) && FileLock.IsLocked(filePath, checkWriteAccess: true))
			{
				WorkerLog.Instance.Warning($"File '{filePath}' is locked for writing. unable overwrite file");
			}
			else
			{
				WorkerLog.Instance.Information($"Decompressing file '{tempPath}'");
				await file.DecompressFrom(tempPath);
				WorkerLog.Instance.Information($"File decompressed to '{filePath}'");
				await MarkFileAsCompleted(file);
			}

			DeleteTempPath(tempPath);
		}

		private async Task ProcessChange(File file, EofMessage eofMessage)
		{
			var tempPath = await BuildFile(file, eofMessage);
			if (tempPath is null)
				return;

			var filePath = file.GetFullPath();
			if (System.IO.File.Exists(filePath) && !FileLock.IsLocked(filePath, checkWriteAccess: true))
			{
				WorkerLog.Instance.Information($"Applying delta to {filePath}");
				file.ApplyDelta(System.IO.File.ReadAllBytes(tempPath));
				await MarkFileAsCompleted(file);

			}
			else
			{
				WorkerLog.Instance.Warning($"File '{filePath}' does not exist or is locked for writing. unable to apply delta");
			}
			DeleteTempPath(tempPath);
		}

		private async Task<string> BuildFile(File file, EofMessage eofMessage)
		{
			try
			{
				WorkerLog.Instance.Information($"Building {(file.IsNew() ? "" : "delta")} file '{file.Name}'");
				var temppath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
				using var sw = System.IO.File.OpenWrite(temppath);

				var toTake = 10;
				var toSkip = 0;
				while (toSkip < eofMessage.AmountOfChunks)
				{
					//best way is to get the chunks in chunks. If a file exists out of 1000 * 1Mb files and load that into memory, you are gonna have a bad time
					var chunks = await _fileChunkRepository.GetFileChunkData(eofMessage.FileId, toSkip, toTake, _database);
					foreach (var chunk in chunks.Data)
						await sw.WriteAsync(chunk, 0, chunk.Length);
					toSkip = toTake + 1;
					toTake += 10;
				}
				WorkerLog.Instance.Information($"File built to '{temppath}'");
				return temppath;
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Error(ex, $"Error occured while assembling {(file.IsNew() ? "" : "delta")} file '{file.Name}' to tempfile");
				return null;
			}
		}

		private void DeleteTempPath(string tempPath)
		{
			try
			{
				WorkerLog.Instance.Information($"Deleting temp file '{tempPath}'");
				System.IO.File.Delete(tempPath);
			}
			catch (Exception ex)
			{
				WorkerLog.Instance.Warning(ex, "Error occured while deleting temp file of assembled file");
			}
		}

		private async Task MarkFileAsCompleted(File file)
		{
			WorkerLog.Instance.Information($"Mark '{file.Path}' as completed, set signature and deleting chunks");
			var filePath = file.GetFullPath();
			var signature = file.CreateSignature();
			var newCreationDate = System.IO.File.GetLastWriteTimeUtc(filePath);
			await _fileRepository.UpdateFileAsAssembled(file.Id, newCreationDate, signature, _database);
			await _fileChunkRepository.DeleteChunksForFile(file.Id, _database);
		}
	}

	public class FileAssemblerServiceFactory
	{
		private readonly IDatabaseFactory _databaseFactory;
		private readonly IFileRepository _fileRepository;
		private readonly IFileChunkRepository _fileChunkRepository;

		public FileAssemblerServiceFactory(IDatabaseFactory databaseFactory
			, IFileRepository fileRepository
			, IFileChunkRepository fileChunkRepository)
		{
			_databaseFactory = databaseFactory;
			_fileRepository = fileRepository;
			_fileChunkRepository = fileChunkRepository;
		}

		//to make sure that every thread get's its own instance of IDatabase
		public FileAssemblerService Get()
		{
			return new FileAssemblerService(_databaseFactory.Get(), _fileRepository, _fileChunkRepository);
		}
	}
}