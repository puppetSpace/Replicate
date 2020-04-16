using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Common.Interfaces.Repositories;
using Pi.Replicate.Data.Db.Repositories;
using Pi.Replicate.Domain;
using System;

namespace Pi.Replicate.Data.Db
{
	public class WorkerContext : IWorkerContext
	{
		private readonly string _connectionString;
		public WorkerContext(IConfiguration configuration)
		{
			_connectionString = configuration.GetConnectionString("ReplicateDatabase");
			FolderRepository = new FolderRepository(_connectionString);
			FileRepository = new FileRepository(_connectionString);
			FileChunkRepository = new FileChunkRepository(_connectionString);
			ChunkPackageRepository = new ChunkPackageRepository(_connectionString);
			RecipientRepository = new RecipientRepository(_connectionString);
			FailedFileRepository = new FailedFileRepository(_connectionString);
		}


		public IFolderRepository FolderRepository { get; }
		public IFileRepository FileRepository { get; }
		public IFileChunkRepository FileChunkRepository { get; }
		public IChunkPackageRepository ChunkPackageRepository { get; }
		public IRecipientRepository RecipientRepository { get; }
		public IFailedFileRepository FailedFileRepository { get; }

		public SqlConnection BuildConnection()
		{
			return new SqlConnection(_connectionString);
		}
	}
}
