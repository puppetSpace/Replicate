using Pi.Replicate.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public class Startup
    {
        private readonly IDatabase _database;

        public Startup(IDatabase database)
        {
            _database = database;
        }
        public async Task Initialize()
        {
            await _database.Execute("DELETE FROM dbo.FileChunk where fileId in (select id from dbo.[File] where status in (0,1))", null);
            await _database.Execute("DELETE FROM dbo.ChunkPackage where fileChunkId in (select id from dbo.FileChunk where fileId in (select id from dbo.[File] where status in (2)))", null);
            await _database.Execute("DELETE FROM dbo.[File] where status in (0,1)", null);

            //todo add filechunks to queue for file that has been processed
        }
    }
}
