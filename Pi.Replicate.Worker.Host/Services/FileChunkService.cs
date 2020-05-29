using Pi.Replicate.Domain;
using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	//no need to use repositories here. Passing the byte array to too many methods can increase memory
	public class FileChunkService
	{
		private readonly IDatabase _database;
		private const string _insertStatementAddReceivedFileChunk = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.FileChunk WHERE FileId = @FileId and SequenceNo = @SequenceNo)
				INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,[Value]) VALUES(@Id,@FileId,@SequenceNo,@Value)
			ELSE
				UPDATE dbo.FileChunk SET [Value] = @Value WHERE FileId = @FileId and SequenceNo = @SequenceNo";
		private const string _recipientCreationStatementAddReceivedFileChunk = @"
			BEGIN
				DECLARE @recipientId uniqueidentifier;

				SELECT @recipientId = Id
				FROM dbo.Recipient
				WHERE [Name] = @Name;

				IF(@recipientId is null)
				BEGIN
					SET @recipientId = NEWID();
					INSERT INTO dbo.Recipient(Id,[Name],[Address], Verified) VALUES(@recipientId,@Name,@Address,1);
				END

				SELECT @recipientId;
			END";
		private const string _insertTransmissionStatementAddReceivedFileChunk = "INSERT INTO dbo.TransmissionResult(Id,RecipientId, FileId,FileChunkSequenceNo, Source) VALUES(NEWID(),@RecipientId,@FileId, @FileChunkSequenceNo, @Source)";

		public FileChunkService(IDatabase database)
		{
			_database = database;
		}
		public async Task<Result> AddReceivedFileChunk(Guid fileId, int sequenceNo, byte[] value, string sender, string senderAddress)
		{
			using (_database)
			{
				var fileChunk = await _database.Execute(_insertStatementAddReceivedFileChunk, new { Id = Guid.NewGuid(), FileId = fileId, SequenceNo = sequenceNo, Value = value });
				var recipientId = await _database.Execute<Guid>(_recipientCreationStatementAddReceivedFileChunk, new { Name = sender, Address = senderAddress });
				var transmissionMessage = await _database.Execute(_insertTransmissionStatementAddReceivedFileChunk, new { RecipientId = recipientId.Data, FileId = fileId, FileChunkSequenceNo = sequenceNo, Source = FileSource.Remote });
				return fileChunk.WasSuccessful && recipientId.WasSuccessful && transmissionMessage.WasSuccessful ? Result.Success() : Result.Failure();
			}
		}
	}
}
