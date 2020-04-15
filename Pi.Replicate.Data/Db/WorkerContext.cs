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
		private readonly SqlConnection _sqlConnection;
		public WorkerContext(IConfiguration configuration)
		{
			_sqlConnection = new SqlConnection(configuration.GetConnectionString("ReplicateDatabase"));
			FolderRepository = new FolderRepository(_sqlConnection);
			FileRepository = new FileRepository(_sqlConnection);
			FileChunkRepository = new FileChunkRepository(_sqlConnection);
			ChunkPackageRepository = new ChunkPackageRepository(_sqlConnection);
			RecipientRepository = new RecipientRepository(_sqlConnection);
			FailedFileRepository = new FailedFileRepository(_sqlConnection);
		}


		public IFolderRepository FolderRepository { get; }
		public IFileRepository FileRepository { get; }
		public IFileChunkRepository FileChunkRepository { get; }
		public IChunkPackageRepository ChunkPackageRepository { get; }
		public IRecipientRepository RecipientRepository { get; }
		public IFailedFileRepository FailedFileRepository { get; }

		public void Dispose()
		{
			_sqlConnection?.Close();
		}
	}
}
