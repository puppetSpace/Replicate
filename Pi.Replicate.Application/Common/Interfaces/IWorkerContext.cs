using Microsoft.EntityFrameworkCore;
using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Interfaces
{
    public interface IWorkerContext
    {
        DbSet<Folder> Folders { get; set; }
        DbSet<File> Files { get; set; }
        DbSet<FileChunk> FileChunks { get; set; }
        DbSet<ChunkPackage> ChunkPackages { get; set; }

        DbSet<Recipient> Recipients { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
