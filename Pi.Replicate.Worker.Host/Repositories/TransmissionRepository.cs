using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public class TransmissionRepository
	{
		private readonly IDatabase _database;

		private const string _insertStatementAddTransmissionResult = "INSERT INTO dbo.TransmissionResult(Id,RecipientId, FileId,FileChunkSequenceNo, Source) VALUES(NEWID(),@RecipientId,@FileId, @FileChunkSequenceNo, @Source)";
		private const string _insertStatementAddFailedFileTransmission = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE FileId = @FileId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileId) VALUES(@Id,@RecipientId,@FileId)";
		private const string _insertStatementAddFailedEofMessageTransmission = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE EofMessageId = @EofMessageId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,EofMessageId) VALUES(@Id,@RecipientId,@EofMessageId)";
		private const string _insertStatementAddFailedFileChunkTransmission = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.FailedTransmission WHERE FileChunkId = @FileChunkId and RecipientId = @RecipientId)
					INSERT INTO dbo.FailedTransmission(Id,RecipientId,FileChunkId) VALUES(@Id,@RecipientId,@FileChunkId)";
		private const string _selectStatementGetFailedFileTransmission = @"SELECT fi.Id, fi.FolderId, fi.Version, fi.LastModifiedDate, fi.Name, fi.Path, fi.Size, fi.Source 
												,fo.Id, fo.Name
												,re.Id, re.Name, re.Address
					FROM dbo.[File] fi
					INNER JOIN dbo.FailedTransmission ftn on ftn.FileId = fi.id
					LEFT JOIN dbo.Folder fo on fo.Id = fi.FolderId
					LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId
					WHERE fi.Source = 0";
		private const string _selectStatementGetFailedEofMessageTransmission = @"SELECT em.Id,em.FileId,em.AmountOfChunks, em.CreationTime
												  , re.Id, re.Name, re.Address
				FROM dbo.EofMessage em
				INNER JOIN dbo.FailedTransmission ftn on ftn.EofMessageId = em.Id
				LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId";
		private const string _selectStatementGetFailedFileChunkTransmission = @"SELECT fc.Id,fc.FileId,fc.SequenceNo, fc.Value
												  , re.Id, re.Name, re.Address
				FROM dbo.FileChunk fc
				INNER JOIN dbo.FailedTransmission ftn on ftn.FileChunkId = fc.Id
				LEFT JOIN dbo.Recipient re on re.Id = ftn.RecipientId";
		private const string _deleteStatementDeleteFailedFileTransmission = "DELETE FROM dbo.FailedTransmission WHERE FileId = @FileId and RecipientId = @RecipientId";
		private const string _deleteStatementDeleteFailedEofMessageTransmission = "DELETE FROM dbo.FailedTransmission WHERE EofMessageId = @EofMessageId and RecipientId = @RecipientId";
		private const string _deleteStatementDeleteFailedFileChunkTransmission = "DELETE FROM dbo.FailedTransmission WHERE FileChunkId = @FileChunkId and RecipientId = @RecipientId";
		private const string _deleteFileChunkStatement = "DELETE FROM dbo.FileChunk WHERE FileChunkId = @FileChunkId";

		private const string _insertStatementFileChunk = @"IF NOT EXISTS(SELECT 1 FROM dbo.FileChunk WHERE Id = @Id)
															BEGIN
																INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,Value) VALUES(@Id,@FileId,@SequenceNo,@Value)
															END";

		public TransmissionRepository(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> AddTransmissionResult(Guid fileId, Guid recipientId, int fileChunkSequenceNo, FileSource fileSource)
		{
			using (_database)
			{
				return await _database.Execute(_insertStatementAddTransmissionResult, new { RecipientId = recipientId, FileId = fileId, FileChunkSequenceNo = fileChunkSequenceNo, Source = fileSource });
			}
		}

		public async Task<Result> AddFailedFileTransmission(Guid fileId, Guid recipientId)
		{
			using (_database)
			{
				return await _database.Execute(_insertStatementAddFailedFileTransmission, new { Id = Guid.NewGuid(), recipientId, FileId = fileId });
			}
		}

		public async Task<Result> AddFailedEofMessageTransmission(Guid eofMessageId, Guid recipientId)
		{
			using (_database)
			{
				return await _database.Execute(_insertStatementAddFailedEofMessageTransmission, new { Id = Guid.NewGuid(), recipientId, EofMessageId = eofMessageId });
			}
		}

		public async Task<Result> AddFailedFileChunkTransmission(Guid fileChunkId, Guid fileId, Guid recipientId, int sequenceNo, byte[] value)
		{
			using (_database)
			{
				//the filechunk must be save so it can be retrieved again for resend
				var resultFileChunk = await _database.Execute(_insertStatementFileChunk, new { Id = fileChunkId, FileId = fileId, SequenceNo = sequenceNo, Value = value });
				var resultFailedFileChunk = await _database.Execute(_insertStatementAddFailedFileChunkTransmission, new { Id = Guid.NewGuid(), RecipientId = recipientId, FileChunkId = fileChunkId });

				return resultFailedFileChunk.WasSuccessful && resultFileChunk.WasSuccessful ? Result.Success() : Result.Failure();
			}
		}

		public async Task<Result<ICollection<(File, Folder, Recipient)>>> GetFailedFileTransmission()
		{
			using (_database)
			{
				return await _database.Query<File, Folder, Recipient, (File, Folder, Recipient)>(_selectStatementGetFailedFileTransmission, null
				, (fi, fo, re) => (fi, fo, re));

			}
		}

		public async Task<Result<ICollection<(EofMessage, Recipient)>>> GetFailedEofMessageTransmission()
		{
			using (_database)
			{
				return await _database.Query<EofMessage, Recipient, (EofMessage, Recipient)>(_selectStatementGetFailedEofMessageTransmission, null
				, (em, re) => (em, re));
			}
		}

		public async Task<Result<ICollection<(FileChunk, Recipient)>>> GetFailedFileChunkTransmission()
		{
			using (_database)
			{
				return await _database.Query<FileChunk, Recipient, (FileChunk, Recipient)>(_selectStatementGetFailedFileChunkTransmission, null
				, (fc, re) => (fc, re));
			}
		}

		public async Task<Result> DeleteFailedFileTransmission(Guid fileId, Guid recipientId)
		{
			using (_database)
				return await _database.Execute(_deleteStatementDeleteFailedFileTransmission, new { FileId = fileId, RecipientId = recipientId });
		}

		public async Task<Result> DeleteFailedEofMessageTransmission(Guid eofMessageId, Guid recipientId)
		{
			using (_database)
				return await _database.Execute(_deleteStatementDeleteFailedEofMessageTransmission, new { EofMessageId = eofMessageId, RecipientId = recipientId });
		}

		public async Task<Result> DeleteFailedFileChunkTransmission(Guid fileChunkId, Guid recipientId)
		{
			using (_database)
			{
				var resultFt = await _database.Execute(_deleteStatementDeleteFailedFileChunkTransmission, new { FileChunkId = fileChunkId, RecipientId = recipientId });
				var resultFc = await _database.Execute(_deleteFileChunkStatement, new { FileChunkId = fileChunkId });

				return resultFc.WasSuccessful && resultFt.WasSuccessful ? Result.Success() : Result.Failure();
			}
		}

	}
}
