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
		private readonly string _connectionString;

		public FileChunkRepository(string connectionString)
		{
			_connectionString = connectionString;
		}

		/// <summary>
		/// Add a file chunk to the database. If you will insert alot of file chunks, it is best to use the overload that accepts a SqlConnection.
		/// Else a new connection will be opened for every insert
		/// </summary>
		/// <param name="fileChunk"></param>
		/// <returns></returns>
		public async Task Create(FileChunk fileChunk)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				await Create(fileChunk, sqlConnection);
			}
		}

		/// <summary>
		/// Add a file chunk to the database. If you will insert alot of file chunks, it is best to pass along a SqlConnection that stays open
		/// for the duration of the insert. Else a new connection will be opened for every insert
		/// </summary>
		/// <param name="fileChunk">chunk of a file</param>
		/// <param name="sqlConnection">connection to the database</param>
		/// <returns></returns>
		public async Task Create(FileChunk fileChunk, SqlConnection sqlConnection)
		{
			await sqlConnection.ExecuteAsync("INSERT INTO dbo.FileChunks(Id,FileId,SequenceNo,Value,ChunkSource) VALUES (@Id,@FileId,@SequenceNo,@Value,@ChunkSource)",
			new { fileChunk.Id, fileChunk.FileId, fileChunk.SequenceNo, fileChunk.Value, fileChunk.ChunkSource });
		}

		public async Task<ICollection<FileChunk>> GetForFile(Guid fileId, int minSequenceNo = 0, int maxSequenceNo = int.MaxValue)
		{
			using (var sqlConnection = new SqlConnection(_connectionString))
			{
				var result = await sqlConnection.QueryAsync<FileChunk>("SELECT Id, FileId,SequenceNo,Value,ChunkSource WHERE FileId = @FileId and SequenceNo between @MinSequenceNo and @MaxSequenceNo", new { FileId = fileId, MinSequenceNo = minSequenceNo, MaxSequenceNo = maxSequenceNo });
				return result.ToList();
			}
		}
	}
}
