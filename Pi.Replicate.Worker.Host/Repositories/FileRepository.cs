using Pi.Replicate.Shared.Models;
using Pi.Replicate.Worker.Host.Data;
using Pi.Replicate.Worker.Host.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Repositories
{
	public interface IFileRepository
	{
		Task<Result> AddNewFile(File file);
		Task<Result> AddNewFile(File file, byte[] signature);
		Task<Result<ICollection<(File, EofMessage)>>> GetCompletedFiles();
		Task<Result<ICollection<File>>> GetFailedFiles();
		Task<Result<ICollection<File>>> GetFilesForFolder(Guid folderId);
		Task<Result<File>> GetLastVersionOfFile(Guid folderId, string relativePath);
		Task<Result<byte[]>> GetSignatureOfPreviousFile(Guid fileId);
		Task<Result> UpdateFileAsFailed(Guid fileId);
		Task<Result> UpdateFileAsAssembled(Guid fileId, DateTime lastModifiedDate, byte[] signature, IDatabase database = null);
		Task<Result<ICollection<File>>> GetAllVersionsOfFile(File file, IDatabase database = null);
	}

	public class FileRepository : IFileRepository
	{
		private readonly IDatabaseFactory _database;

		private const string _insertStatementFile = @"
				IF NOT EXISTS (SELECT 1 FROM dbo.[File] WHERE Id = @Id)
					INSERT INTO dbo.[File](Id,FolderId, Name, Size,Version,LastModifiedDate,Path,Signature, Source) VALUES(@Id,@FolderId,@Name,@Size, @Version, @LastModifiedDate,@Path, @Signature, @Source)";
		private const string _selectStatementGetLastVersionOfFile = "SELECT TOP 1 Id, FolderId,Version, LastModifiedDate, Name,Path,Size,Source FROM dbo.[File] WHERE FolderId = @FolderId and Path = @Path order by Version desc";
		private const string _selectStatementGetFilesForFolder = @"
			select Id,FolderId,Name,Version,Size,LastModifiedDate,Path,Source
			from (
				select *,ROW_NUMBER() over(partition by Name order by Version desc)  rnk
				from dbo.[File]
				where folderId = @FolderId
			) a
			where rnk = 1";
		private const string _selectStatementGetSignatureOfPreviousFile = @"
			WITH file_cte([path],[version])
			AS (SELECT [path],[version] FROM dbo.[File] WHERE Id = @FileId)
			SELECT [signature] 
			FROM dbo.[File] fi
			INENR JOIN file_cte fic ON fic.[path] = fi.[Path] AND fi.[Version] = fic.[version] - 1 ";
		private const string _updateStatementUpdateFileAsFailed = @"UPDATE dbo.[File] SET [Status] = 1 WHERE Id = @Id";
		private const string _updateStatementUpdateFileAssAssembled = "UPDATE dbo.[File] SET [Status] = 2, Signature = @Signature, LastModifiedDate = @LastModifiedDate WHERE Id = @FileId";
		private const string _selectStatementGetCompletedFiles = @"
			select fi.Id, fi.FolderId,fi.[Name],fi.[Version], fi.Size,fi.LastModifiedDate, fi.[Path], fi.Source
			, eme.Id,eme.FileId,eme.AmountOfChunks,eme.CreationTime
			from dbo.[File] fi
			inner join dbo.EofMessage eme on eme.FileId = fi.Id
			left join dbo.FileChunk fc on fc.FileId = fi.Id
			where fi.Source = 1
			and fi.Id not in (select fileId from dbo.FileConflict)
			group by fi.Id, fi.FolderId,fi.[Name],fi.[Version], fi.Size,fi.LastModifiedDate, fi.[Path], fi.Source
			, eme.Id,eme.FileId,eme.AmountOfChunks,eme.CreationTime
			having sum(fc.SequenceNo) = (eme.AmountOfChunks*(eme.AmountOfChunks+1)) / 2";
		private const string _selectStatementGetFailedFiles = "SELECT Id,FolderId,Name,Version,Size,LastModifiedDate,Path,Source FROM dbo.[File] WHERE [Status] = 1";
		private const string _selectStatementGetOtherVersionsOfFile = "SELECT Id,FolderId,Name,Version,Size,LastModifiedDate,Path,Source FROM dbo.[File] WHERE [Path] = @Path";

		public FileRepository(IDatabaseFactory database)
		{
			_database = database;
		}


		public async Task<Result> AddNewFile(File file, byte[] signature)
		{
			var db = _database.Get();
			using (db)
				return await db.Execute(_insertStatementFile, new { file.Id, file.FolderId, file.Name, file.Size, file.Version, file.LastModifiedDate, file.Path, Signature = signature, file.Source });
		}

		public async Task<Result> AddNewFile(File file)
		{
			return await AddNewFile(file, null);
		}

		public async Task<Result<File>> GetLastVersionOfFile(Guid folderId, string relativePath)
		{
			var db = _database.Get();
			using (db)
				return await db.QuerySingle<File>(_selectStatementGetLastVersionOfFile, new { FolderId = folderId, Path = relativePath });
		}

		public async Task<Result<ICollection<File>>> GetFilesForFolder(Guid folderId)
		{
			var db = _database.Get();
			using (db)
				return await db.Query<File>(_selectStatementGetFilesForFolder, new { FolderId = folderId });
		}

		public async Task<Result<byte[]>> GetSignatureOfPreviousFile(Guid fileId)
		{
			var db = _database.Get();
			using (db)
				return await db.QuerySingle<byte[]>(_selectStatementGetSignatureOfPreviousFile, new { FileId = fileId });
		}

		public async Task<Result<ICollection<(File, EofMessage)>>> GetCompletedFiles()
		{
			var db = _database.Get();
			using (db)
			{
				return await db.Query<File, EofMessage, (File file, EofMessage eofMessage)>(_selectStatementGetCompletedFiles, null
					, (f, e) => (f, e));
			}
		}

		public async Task<Result<ICollection<File>>> GetFailedFiles()
		{
			var db = _database.Get();
			using (db)
				return await db.Query<File>(_selectStatementGetFailedFiles, null);

		}

		public async Task<Result> UpdateFileAsFailed(Guid fileId)
		{
			var db = _database.Get();
			using (db)
				return await db.Execute(_updateStatementUpdateFileAsFailed, new { Id = fileId });
		}

		public async Task<Result<ICollection<File>>> GetAllVersionsOfFile(File file, IDatabase database = null)
		{
			if (database is null)
			{
				var db = _database.Get();
				using (db)
					return await db.Query<File>(_selectStatementGetOtherVersionsOfFile, new { file.Path, file.Version });
			}
			else
			{
				return await database.Query<File>(_selectStatementGetOtherVersionsOfFile, new { file.Path, file.Version });
			}
		}

		public async Task<Result> UpdateFileAsAssembled(Guid fileId, DateTime lastModifiedDate, byte[] signature, IDatabase database = null)
		{
			if (_database is null)
			{
				var db = _database.Get();
				using (db)
					return await db.Execute(_updateStatementUpdateFileAssAssembled, new { FileId = fileId, Signature = signature, LastModifiedDate = lastModifiedDate });
			}
			else
			{
				return await database.Execute(_updateStatementUpdateFileAssAssembled, new { FileId = fileId, Signature = signature, LastModifiedDate = lastModifiedDate });
			}
		}
	}
}
