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
    public class ConflictRepository
    {
		private readonly IDatabaseFactory _databaseFactory;
		private const string _insertStatementAddConflict = "INSERT INTO dbo.FileConflict(Id,FileId,Type) VALUES(NEWID(),@FileId,@Type)";

		public ConflictRepository(IDatabaseFactory databaseFactory)
		{
			_databaseFactory = databaseFactory;
		} 
		

		public async Task<Result> AddConflict(FileConflict fileConflict)
		{
			var db = _databaseFactory.Get();
			using (db)
				return await db.Execute(_insertStatementAddConflict, new {fileConflict.FileId, fileConflict.Type });
		}
    }
}
