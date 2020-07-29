using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public interface IFileChunkRepository
	{
		Task<Result> AddReceivedFileChunk(ReceivedFileChunk receivedFileChunk);
		Task<Result<ICollection<byte[]>>> GetFileChunkData(Guid fileId, int toSkip, int toTake, IDatabase database = null);
		Task<Result> DeleteChunksForFile(Guid fileId, IDatabase database = null);
	}

	public class FileChunkRepository : IFileChunkRepository
	{
		private readonly IDatabaseFactory _databaseFactory;

		private const string _insertStatementAddReceivedFileChunk = @"
BEGIN
			DECLARE @RecipientId uniqueidentifier;

			--add chunk
			IF NOT EXISTS (SELECT 1 FROM dbo.FileChunk WHERE FileId = @FileId and SequenceNo = @SequenceNo)
				INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,[Value]) VALUES(@Id,@FileId,@SequenceNo,@Value)
			ELSE
				UPDATE dbo.FileChunk SET [Value] = @Value WHERE FileId = @FileId and SequenceNo = @SequenceNo
			
			--add recipient
			SELECT @RecipientId = Id
			FROM dbo.Recipient
			WHERE [Name] = @RecipientName;

			IF(@recipientId is null)
			BEGIN
				SET @recipientId = NEWID();
				INSERT INTO dbo.Recipient(Id,[Name],[Address], Verified) VALUES(@RecipientId,@RecipientName,@RecipientAddress,0);
			END
			
			--add transmissionresult
			INSERT INTO dbo.TransmissionResult(Id,RecipientId, FileId,FileChunkSequenceNo, Source) VALUES(NEWID(),@RecipientId,@FileId, @SequenceNo, @Source)
END";
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

		public async Task<Result> AddReceivedFileChunk(ReceivedFileChunk receivedFileChunk)
		{
			var db = _databaseFactory.Get();
			using (db)
				return await db.Execute(_insertStatementAddReceivedFileChunk, 
					new 
					{ 
						  Id = receivedFileChunk.Id
						, FileId = receivedFileChunk.FileId
						, SequenceNo = receivedFileChunk.SequenceNo
						, Value = receivedFileChunk.GetValue()
						, RecipientName = receivedFileChunk.Sender
						, RecipientAddress = receivedFileChunk.SenderAddress
						, Source = FileSource.Remote 
					});
		}
	}
}
