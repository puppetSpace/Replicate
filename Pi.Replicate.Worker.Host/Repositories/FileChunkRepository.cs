using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public interface IFileChunkRepository
	{
		Task<Result<ICollection<byte[]>>> GetFileChunkData(Guid fileId, int toSkip, int toTake, IDatabase database = null);
		Task<Result> DeleteChunksForFile(Guid fileId, IDatabase database = null);
	}

	public class FileChunkRepository : IFileChunkRepository
	{
		private readonly IDatabaseFactory _databaseFactory;

		private const string _selectStatementGetFileChunkData = "SELECT [Value] FROM dbo.FileChunk WHERE FileId = @FileId and SequenceNo between @toSkip and @ToTake ORDER BY SEQUENCENO";
		private const string _deleteStatementDeleteFileChunksForFile = "DELETE FROM dbo.FileChunk WHERE FileId = @FileId";

		public FileChunkRepository(IDatabaseFactory databaseFactory)
		{
			_databaseFactory = databaseFactory;
		}

		public async Task<Result<ICollection<byte[]>>> GetFileChunkData(Guid fileId, int toSkip, int toTake, IDatabase database = null)
		{
			if (database is null)
			{
				var db = _databaseFactory.Get();
				using (db)
					return await db.Query<byte[]>(_selectStatementGetFileChunkData, new { FileId = fileId, ToSkip = toSkip, ToTake = toTake });
			}
			else
			{
				return await database.Query<byte[]>(_selectStatementGetFileChunkData, new { FileId = fileId, ToSkip = toSkip, ToTake = toTake });
			}
		}

		public async Task<Result> DeleteChunksForFile(Guid fileId, IDatabase database = null)
		{
			if (database is null)
			{
				var db = _databaseFactory.Get();
				using (db)
					return await database.Execute(_deleteStatementDeleteFileChunksForFile, new { FileId = fileId });
			}
			else
			{
				return await database.Execute(_deleteStatementDeleteFileChunksForFile, new { FileId = fileId });
			}
		}
	}
}
