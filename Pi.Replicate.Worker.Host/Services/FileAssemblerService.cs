using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Shared;
using Pi.Replicate.Worker.Host.Models;
using Serilog;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
[assembly: InternalsVisibleTo("Pi.Replicate.Test")]
namespace Pi.Replicate.Worker.Host.Services
{
	public class FileAssemblerService
	{
		private readonly ICompressionService _compressionService;
		private readonly PathBuilder _pathBuilder;
		private readonly IDeltaService _deltaService;
		private readonly IDatabase _database;
		private readonly IWebhookService _webhookService;

		public FileAssemblerService(ICompressionService compressionService
			, PathBuilder pathBuilder
			, IDeltaService deltaService
			, IDatabase database
			, IWebhookService webhookService)
		{
			_pathBuilder = pathBuilder;
			_compressionService = compressionService;
			_deltaService = deltaService;
			_database = database;
			_webhookService = webhookService;
		}

		public async Task ProcessFile(File file, EofMessage eofMessage)
		{
			try
			{
				if (file.IsNew())
					await ProcessNew(file, eofMessage);
				else
					await ProcessChange(file, eofMessage);
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Unexpected error occured while assembling the file '{file.Name}'");
			}
		}

		private async Task ProcessNew(File file, EofMessage eofMessage)
		{
			using (_database)
			{
				var tempPath = await BuildFile(file, eofMessage);
				if (tempPath is null)
					return;

				var filePath = _pathBuilder.BuildPath(file.Path);

				if (System.IO.File.Exists(filePath) && FileLock.IsLocked(filePath, checkWriteAccess: true))
				{
					Log.Warning($"File '{filePath}' is locked for writing. unable overwrite file");
				}
				else
				{
					Log.Information($"Decompressing file '{tempPath}'");
					await _compressionService.Decompress(tempPath, filePath);
					Log.Information($"File decompressed to '{filePath}'");
					await MarkFileAsCompleted(file);
				}

				DeleteTempPath(tempPath);
			}
		}

		private async Task ProcessChange(File file, EofMessage eofMessage)
		{
			using (_database)
			{
				var tempPath = await BuildFile(file, eofMessage);
				if (tempPath is null)
					return;

				var filePath = _pathBuilder.BuildPath(file.Path);
				if (System.IO.File.Exists(filePath) && !FileLock.IsLocked(filePath, checkWriteAccess: true))
				{
					Log.Information($"Applying delta to {filePath}");
					_deltaService.ApplyDelta(filePath, System.IO.File.ReadAllBytes(filePath));
					await MarkFileAsCompleted(file);

				}
				else
				{
					Log.Warning($"File '{filePath}' does not exist or is locked for writing. unable to apply delta");
				}
				DeleteTempPath(tempPath);
			}
		}

		private async Task<string> BuildFile(File file, EofMessage eofMessage)
		{
			try
			{
				Log.Information($"Building {(file.IsNew() ? "" : "delta")} file '{file.Name}'");
				var temppath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
				using var db = _database;
				using var sw = System.IO.File.OpenWrite(temppath);

				var toTake = 10;
				var toSkip = 0;
				while (toSkip < eofMessage.AmountOfChunks)
				{
					//best way is to get the chunks in chunks. If a file exists out of 1000 * 1Mb files and load that into memory, you are gonna have a bad time
					var chunks = await db.Query<byte[]>("SELECT [Value] FROM dbo.FileChunk WHERE FileId = @FileId and SequenceNo between @toSkip and @ToTake ORDER BY SEQUENCENO", new { FileId = eofMessage.FileId, ToSkip = toSkip, ToTake = toTake });
					foreach (var chunk in chunks)
						await sw.WriteAsync(chunk, 0, chunk.Length);
					toSkip = toTake + 1;
					toTake += 10;
				}
				Log.Information($"File built to '{temppath}'");
				return temppath;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error occured while assembling {(file.IsNew() ? "" : "delta")} file '{file.Name}' to tempfile");
				return null;
			}
		}

		private void DeleteTempPath(string tempPath)
		{
			try
			{
				Log.Information($"Deleting temp file '{tempPath}'");
				System.IO.File.Delete(tempPath);
			}
			catch (Exception ex)
			{
				Log.Warning(ex, "Error occured while deleting temp file of assembled file");
			}
		}

		private async Task MarkFileAsCompleted(File file)
		{
			Log.Information($"Mark '{file.Path}' as completed, set signature and deleting chunks");
			var filePath = _pathBuilder.BuildPath(file.Path);
			var signature = _deltaService.CreateSignature(filePath);
			await _database.Execute("UPDATE dbo.[File] SET [Status] = 2, Signature = @Signature WHERE Id = @FileId", new { FileId = file.Id, Signature = signature.ToArray() });
			await _database.Execute("DELETE FROM dbo.FileChunk WHERE FileId = @FileId", new { FileId = file.Id });
			_webhookService.NotifyFileAssembled(file);
		}
	}

	public class FileAssemblerServiceFactory
	{
		private readonly ICompressionService _compressionService;
		private readonly PathBuilder _pathBuilder;
		private readonly IDeltaService _deltaService;
		private readonly IDatabaseFactory _databaseFactory;
		private readonly IWebhookService _webhookService;

		public FileAssemblerServiceFactory(ICompressionService compressionService
			, PathBuilder pathBuilder
			, IDeltaService deltaService
			, IDatabaseFactory databaseFactory
			, IWebhookService webhookService)
		{
			_compressionService = compressionService;
			_pathBuilder = pathBuilder;
			_deltaService = deltaService;
			_databaseFactory = databaseFactory;
			_webhookService = webhookService;
		}

		//to make sure that every thread get's its own instance of IDatabase
		public FileAssemblerService Get()
		{
			return new FileAssemblerService(_compressionService, _pathBuilder, _deltaService, _databaseFactory.Get(), _webhookService);
		}
	}
}