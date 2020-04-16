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
	public class ChunkPackageRepository : IChunkPackageRepository
	{
		private readonly string _connectionString;

		public ChunkPackageRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		public async Task<ICollection<ChunkPackage>> Get()
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var result = await sqlConnection.QueryAsync<ChunkPackage, FileChunk, Recipient, ChunkPackage>(@"
				select cp.Id
				, fc.Id, fc.FileId, fc.SequenceNo, fc.Value, fc.ChunkSource
				, re.Id, re.Name, re.Address
				from dbo.ChunkPackages cp
				inner join dbo.FileChunks fc on fc.Id = cp.FileChunkId
				inner join dbo.Recipients re on re.Id = cp.RecipientId"
				, (cp, fc, re) =>
				 {
					 cp.FileChunk = fc;
					 cp.Recipient = re;
					 return cp;
				 });

				return result.ToList();
			}
		}

		public async Task Delete(Guid id)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				await sqlConnection.ExecuteAsync("DELETE FROM db.ChunkPackages WHERE Id = @Id", new { Id = id });
			}
		}

        public async Task Create(ChunkPackage chunkPackage)
        {
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				await sqlConnection.ExecuteAsync("INSERT INTO dbo.ChunkPackages(Id,FileChunkId,RecipientId) VALUES(@Id,@FileChunkId,@RecipientId", new { chunkPackage.Id, FileChunkId = chunkPackage.FileChunk.Id, RecipientId = chunkPackage.Recipient.Id });
			}
        }
    }
}
