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
		private readonly IDatabase _database;

		private const string _insertStatementAddEofMessage = @"
		IF NOT EXISTS (SELECT 1 FROM dbo.EofMessage WHERE FileId = @FileId)	
			INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)";
		private const string _insertStatementAddReceivedEofMessage = @"
			IF NOT EXISTS (SELECT 1 FROM dbo.EofMessage WHERE FileId = @FileId)
				INSERT INTO dbo.EofMessage(Id,FileId,AmountOfChunks) VALUES(@Id,@FileId,@AmountOfChunks)
			ELSE
				UPDATE dbo.EofMessage SET AmountOfChunks = @AmountOfChunks WHERE FileId = @FileID";

		public EofMessageRepository(IDatabase database)
		{
			_database = database;
		}

		public async Task<Result> AddEofMessage(EofMessage eofMessage)
		{
			using (_database)
				return await _database.Execute(_insertStatementAddEofMessage, new { eofMessage.Id, eofMessage.FileId, eofMessage.AmountOfChunks });
		}

		public async Task<Result> AddReceivedEofMessage(EofMessage eofMessage)
		{
			using (_database)
				return await _database.Execute(_insertStatementAddReceivedEofMessage, new { eofMessage.Id, eofMessage.FileId, eofMessage.AmountOfChunks });
		}
	}
}
