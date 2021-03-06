﻿using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public interface IRecipientRepository
	{
		Task<Result> AddRecipientToFolder(string recipientName, string recipientAddress, Guid folderId);
		Task<Result<ICollection<Recipient>>> GetRecipientsForFolder(Guid folderId);
	}

	public class RecipientRepository : IRecipientRepository
	{
		private readonly IDatabaseFactory _database;

		private const string _selectStatementGetRecipientsForFolder = @"
			SELECT re.Id,re.Name,re.Address 
			FROM dbo.Recipient re
			INNER JOIN dbo.FolderRecipient fr on fr.RecipientId = re.Id and fr.FolderId = @FolderId
			WHERE re.Verified = 1";
		private const string _insertStatementAddRecipientToFolder = @"
			BEGIN
				DECLARE @recipientId uniqueidentifier;

				SELECT @recipientId = Id
				FROM dbo.Recipient
				WHERE [Name] = @Name;

				IF(@recipientId is null)
				BEGIN
					SET @recipientId = NEWID();
					INSERT INTO dbo.Recipient(Id,[Name],[Address], Verified) VALUES(@recipientId,@Name,@Address,0);
				END

				IF NOT EXISTS (SELECT 1 FROM dbo.FolderRecipient WHERE FolderId = @FolderId and RecipientId = @recipientId)
					INSERT INTO dbo.FolderRecipient(FolderId,RecipientId) VALUES(@FolderId,@recipientId);
			END";


		public RecipientRepository(IDatabaseFactory database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<Recipient>>> GetRecipientsForFolder(Guid folderId)
		{
			var db = _database.Get();
			using (db)
				return await db.Query<Recipient>(_selectStatementGetRecipientsForFolder, new { FolderId = folderId });
		}

		public async Task<Result> AddRecipientToFolder(string recipientName, string recipientAddress, Guid folderId)
		{
			var db = _database.Get();
			using (db)
				return await db.Execute(_insertStatementAddRecipientToFolder, new { Address = recipientAddress, Name = recipientName, FolderId = folderId });
		}
	}
}
