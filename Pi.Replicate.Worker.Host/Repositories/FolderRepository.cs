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
	public class FolderRepository
	{
		private readonly IDatabaseFactory _database;

		private const string _selectStatementGetFoldersToCrawl = "SELECT Id, Name FROM dbo.Folder";
		private const string _selectStatementGetFolderName = "SELECT Name FROM dbo.Folder WHERE Id = @Id";
		private const string _selectStatementGetFolder = "SELECT Id, Name FROM dbo.Folder WHERE Id = @Id";
		private const string _selectStatementFolderRecipients = @"select re.Id,re.Name,re.Address
				from dbo.FolderRecipient fr
				inner join dbo.Recipient re on re.Id = fr.RecipientId and re.Verified = 1
				WHERE fr.FolderId = @FolderId";
		private const string _insertStatementAddFolder = @"
			BEGIN
				DECLARE @folderId uniqueidentifier;
				SELECT @folderId = id
				FROM dbo.Folder
				WHERE Name = @Name;
				IF (@folderId IS NULL)
					BEGIN
						SET @folderId = NEWID();
						INSERT INTO dbo.Folder(Id,Name) VALUES(@folderId,@Name)
					END

				SELECT @folderId;
			END";

		public FolderRepository(IDatabaseFactory database)
		{
			_database = database;
		}

		public async Task<Result<ICollection<CrawledFolder>>> GetFoldersToCrawl()
		{
			var db = _database.Get();
			using (db)
				return await db.Query<CrawledFolder>(_selectStatementGetFoldersToCrawl, null);
		}

		public async Task<Result<Folder>> GetFolder(Guid folderId)
		{
			var db = _database.Get();
			using (db)
			{
				var folder = await db.QuerySingle<Folder>(_selectStatementGetFolder, new { Id = folderId });
				var recipients = await db.Query<Recipient>(_selectStatementFolderRecipients, new { folderId });
				if (folder.WasSuccessful)
				{
					folder.Data.Recipients = recipients?.Data;
					return Result<Folder>.Success(folder.Data);
				}

				return Result<Folder>.Failure();
			}
		}

		public async Task<Result<string>> GetFolderName(Guid folderId)
		{
			var db = _database.Get();
			using (db)
				return await db.QuerySingle<string>(_selectStatementGetFolderName, new { Id = folderId });
		}

		public async Task<Result<Guid>> AddFolder(string name)
		{
			var db = _database.Get();
			using (db)
				return await db.Execute<Guid>(_insertStatementAddFolder, new { Name = name });
		}
	}
}
