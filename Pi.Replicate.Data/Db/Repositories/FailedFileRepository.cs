using Dapper;
using Microsoft.Data.SqlClient;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data.Db.Repositories
{
	public class FailedFileRepository : IFailedFileRepository
	{
		private readonly string _connectionString;

		public FailedFileRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task Create(FailedFile failedFile)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				await sqlConnection.ExecuteAsync("INSERT INTO dbo.FailedFiles(Id,FileId,RecipientId) VALUES (@Id,@FileId,@RecipientId", new { failedFile.Id, FileId = failedFile.File.Id, RecipientId = failedFile.Recipient.Id });
			}
		}

		public async Task DeleteAll()
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				await sqlConnection.ExecuteAsync("DELETE FROM dbo.FailedFiles");
			}
		}

		public async Task<ICollection<FailedFile>> Get()
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var result = await sqlConnection.QueryAsync<FailedFile, File, Recipient, FailedFile>(@"
				select ff.Id
				,fi.Id, fi.FolderId,fi.AmountOfChunks, fi.Hash, fi.LastModifiedDate, fi.Name,fi.Path, fi.Signature,fi.Size, fi.Status
				, re.Id, re.Name, re.Address
				from dbo.FailedFiles ff
				inner join dbo.Files fi on fi.Id = ff.FileId
				inner join dbo.Recipients re on re.Id = ff.RecipientId",
				(ff, fi, re) =>
				{
					ff.File = fi;
					ff.Recipient = re;
					return ff;
				});

				return result.ToList();
			}
		}
	}
}
