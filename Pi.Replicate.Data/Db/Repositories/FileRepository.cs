using Microsoft.Data.SqlClient;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Pi.Replicate.Data.Db.Repositories
{
	public class FileRepository : IFileRepository
	{
		private SqlConnection _sqlConnection;

		public FileRepository(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
		}

		public async Task<ICollection<File>> GetForFolder(Guid guid)
		{
			var result = await _sqlConnection.QueryAsync<File>("SELECT Id, FolderId,AmountOfChunks, Hash, LastModifiedDate, Name,Path, Signature,Size,Status FROM dbo.Files");
			return result.ToList();
		}

		public async Task Create(File file)
		{
			await _sqlConnection.ExecuteAsync("INSERT INTO dbo.FILE(Id,FolderId, Name, Size,AmountOfChunks,Hash,Status,LastModifiedDate,Path,Signature) VALUES(@Id,@FolderId,@Name,@Size, @AmountOfChunks, @Hash, @Status, @LastModifiedDate,@Path, @Signature)",
				new { file.Id, file.FolderId,file.Name, file.Size, file.AmountOfChunks, file.Hash, file.Status, file.LastModifiedDate,file.Path, file.Signature });
		}

		public async Task Update(File file)
		{
			await _sqlConnection.ExecuteAsync("UPDATE dbo.FILE SET Size = @Size, AmountOfChunks = @AmountOfChunks, Hash = @Hash, Status = @Status, LastModifiedDate = @LastModifiedDate, Signature = @Signature where Id = @Id",
				new { file.Id, file.Size, file.AmountOfChunks, file.Hash, file.Status, file.LastModifiedDate, file.Signature });
		}
	}
}
