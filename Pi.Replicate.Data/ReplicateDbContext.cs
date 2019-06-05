using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Data
{
    public class ReplicateDbContext : DbContext
    {

        public ReplicateDbContext(DbContextOptions<ReplicateDbContext> options):base(options)
        {

        }
        public DbSet<Folder> Folders { get; set; }

		public DbSet<FolderOption> FolderOptions { get; set; }

		public DbSet<File> Files { get; set; }

        public DbSet<FileChunk> FileChunks { get; set; }

        public DbSet<HostFileChunk> FailedUploadFileChunks { get; set; }

        public DbSet<Host> Hosts { get; set; }

        public DbSet<TempFile> TempFiles { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }

    }
}
