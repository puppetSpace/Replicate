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
	public class FileChunkRepository : IFileChunkRepository
	{
		private SqlConnection _sqlConnection;

		public FileChunkRepository(SqlConnection sqlConnection)
		{
			_sqlConnection = sqlConnection;
		}

		public async Task Create(FileChunk fileChunk)
		{
			await _sqlConnection.ExecuteAsync("INSERT INTO dbo.FileChunks(Id,FileId,SequenceNo,Value,ChunkSource) VALUES (@Id,@FileId,@SequenceNo,@Value,@ChunkSource", 
				new { fileChunk.Id, FileId = fileChunk.FileId, fileChunk.SequenceNo, fileChunk.Value, fileChunk.ChunkSource });
		}

        public async Task<ICollection<FileChunk>> GetForFile(Guid fileId)
        {
            var result = await _sqlConnection.QueryAsync<FileChunk>("SELECT Id, FileId,SequenceNo,Value,ChunkSource WHERE FileId = @FileId",new{FileId = fileId});
			return result.ToList();
		}
    }
}
