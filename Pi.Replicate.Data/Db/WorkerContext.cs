using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Domain;
using System;

namespace Pi.Replicate.Data.Db
{
	public class WorkerContext : DbContext, IWorkerContext
	{

		public WorkerContext(DbContextOptions<WorkerContext> options) : base(options)
		{

		}
		public DbSet<Folder> Folders { get; set; }
		public DbSet<File> Files { get; set; }
		public DbSet<FileChunk> FileChunks { get; set; }
		public DbSet<ChunkPackage> ChunkPackages { get; set; }
		public DbSet<Recipient> Recipients { get; set; }
		public DbSet<FailedFile> FailedFiles { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Folder>(x =>
			{
				x.OwnsOne(f => f.FolderOptions);
			});

			modelBuilder.Entity<FolderRecipient>(x =>
			{
				x.HasKey(k => new { k.RecipientId, k.FolderId });
			});
		}
	}
}
