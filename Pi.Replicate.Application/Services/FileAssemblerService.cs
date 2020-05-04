using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;

namespace Pi.Replicate.Application.Services
{
    public class FileAssemblerService
    {
        private readonly CompressionService _compressionService;
        private readonly PathBuilder _pathBuilder;
        private readonly DeltaService _deltaService;
        private readonly IDatabase _database;

        public FileAssemblerService(CompressionService compressionService
            , PathBuilder pathBuilder
            , DeltaService deltaService
            , IDatabase database)
        {
            _pathBuilder = pathBuilder;
            _compressionService = compressionService;
            _deltaService = deltaService;
            _database = database;
        }

        public async Task ProcessFile(File file, EofMessage eofMessage)
        {
			using (_database)
			{
				if (file.IsNew())
					await ProcessNew(file, eofMessage);
				else
					await ProcessChange(file, eofMessage);
			}
        }

        private async Task ProcessNew(File file, EofMessage eofMessage)
        {
            Log.Information($"Building file {file.Name}");
            var tempPath = await BuildFile(eofMessage);
            Log.Information($"File built to '{tempPath}'");

            Log.Information($"Decompressing file '{tempPath}'");
            await _compressionService.Decompress(tempPath, _pathBuilder.BuildPath(file.Path));
            Log.Information($"File decompressed to '{_pathBuilder.BuildPath(file.Path)}'");

            Log.Information($"Deleting temp file '{tempPath}'");
            System.IO.File.Delete(tempPath);
            await DeleteChunks(file);

			await MarkFileAsCompleted(file);
        }

		private async Task ProcessChange(File file, EofMessage eofMessage)
        {
            Log.Information($"Building delta file for {file.Name}");
            var tempPath = await BuildFile(eofMessage);
            Log.Information($"Delta file built to '{tempPath}'");
            var filePath = _pathBuilder.BuildPath(file.Path);
            if (System.IO.File.Exists(filePath) && !FileLock.IsLocked(filePath, checkWriteAccess: true))
            {
                Log.Information($"Applying delta to {filePath}");
                _deltaService.ApplyDelta(filePath, System.IO.File.ReadAllBytes(filePath));
                await DeleteChunks(file);
				await MarkFileAsCompleted(file);

			}
            else
            {
                Log.Warning($"File '{filePath}' does not exist or is locked for writing. unable to apply delta");
            }
            Log.Information($"Deleting temp file '{tempPath}'");
            System.IO.File.Delete(tempPath);
        }

        private async Task<string> BuildFile(EofMessage eofMessage)
        {
            //todo errorhandling
            var temppath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetTempFileName());
            using var db = _database;
            using var sw = System.IO.File.OpenWrite(temppath);

            var toTake = 10;
            var toSkip = 0;
            while (toTake < eofMessage.AmountOfChunks)
            {
                //best way is to get the chunks in chunks. If a file exists out of 1000 * 1Mb files and load that into memory, you are gonna have a bad time
                var chunks = await db.Query<byte[]>("SELECT [Value] FROM dbo.FileChunk WHERE FileId = @FileId and SequenceNo between @toSkip and @ToTake", new { FileId = eofMessage.FileId, ToSkip = toSkip, ToTake = toTake });
                foreach (var chunk in chunks)
                    await sw.WriteAsync(chunk, 0, chunk.Length);
                toSkip = toTake;
                toTake += 10;
            }

            return temppath;
        }

        private async Task DeleteChunks(File file)
        {
            Log.Information($"Deleting chunks for {file.Path}");
            await _database.Execute("DELETE FROM dbo.FileChunk WHERE FileId = @FileId", new { FileId = file.Id });
        }

		private async Task MarkFileAsCompleted(File file)
		{
			Log.Information($"Mark '{file.Path}' as completed");
			await _database.Execute("UPDATE dbo.[File] SET IsReceived = 1 WHERE Id = @FileId", new { FileId = file.Id });
		}
	}

	public class FileAssemblerServiceFactory
	{
		private readonly CompressionService _compressionService;
		private readonly PathBuilder _pathBuilder;
		private readonly DeltaService _deltaService;
		private readonly IDatabaseFactory _databaseFactory;

		public FileAssemblerServiceFactory(CompressionService compressionService
			, PathBuilder pathBuilder
			, DeltaService deltaService
			, IDatabaseFactory databaseFactory)
		{
			_compressionService = compressionService;
			_pathBuilder = pathBuilder;
			_deltaService = deltaService;
			_databaseFactory = databaseFactory;
		}

		//to make sure that every thread get's its own instance of IDatabase
		public FileAssemblerService Get()
		{
			return new FileAssemblerService(_compressionService, _pathBuilder, _deltaService, _databaseFactory.Get());
		}
	}
}