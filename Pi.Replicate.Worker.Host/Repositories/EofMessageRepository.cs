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
	public interface IEofMessageRepository
	{
		Task<Result> AddEofMessage(EofMessage eofMessage);
		Task<Result> AddReceivedEofMessage(EofMessage eofMessage);
	}

	public class EofMessageRepository : IEofMessageRepository
	{
		private readonly IDatabaseFactory _database;

		private const string _insertStatementAddEofMessage = @"
		IF NOT EXISTS (SELECT 1 FROM dbo.EofMessage WHERE FileId = @FileId)	
			INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)";
		private const string _insertStatementAddReceivedEofMessage = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.EofMessage WHERE FileId = @FileId)
				INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)
			ELSE
				UPDATE dbo.EofMessage SET AmountOfChunks = @AmountOfChunks WHERE FileId = @FileID";

		public EofMessageRepository(IDatabaseFactory database)
		{
			_database = database;
		}

		public async Task<Result> AddEofMessage(EofMessage eofMessage)
		{
			var db = _database.Get();
			using (db)
				return await db.Execute(_insertStatementAddEofMessage, new { eofMessage.Id, eofMessage.FileId, eofMessage.AmountOfChunks });
		}

		public async Task<Result> AddReceivedEofMessage(EofMessage eofMessage)
		{
			var db = _database.Get();
			using (db)
				return await db.Execute(_insertStatementAddReceivedEofMessage, new { eofMessage.Id, eofMessage.FileId, eofMessage.AmountOfChunks });
		}
	}
}
