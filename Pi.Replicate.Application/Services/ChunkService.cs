using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Domain;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
    public class ChunkService
    {
        private readonly FileSplitterFactory _fileSplitterFactory;
        private readonly IDatabaseFactory _databaseFactory;
        private const string _insertStatement = "INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,Value,ChunkSource) VALUES (@Id,@FileId,@SequenceNo,@Value,@ChunkSource)";

        public ChunkService(FileSplitterFactory fileSplitterFactory, IDatabaseFactory databaseFactory)
        {
            _fileSplitterFactory = fileSplitterFactory;
            _databaseFactory = databaseFactory;
        }

        public async Task<int> SplitFileIntoChunksAndProduceHash(File file)
        {
            byte[] fileHash;
            int sequenceNo = 0;
            var fileSplitter = _fileSplitterFactory.Get();
            var database = _databaseFactory.Get();
            using (database)
            {
                database.Connection.Open();
                 await fileSplitter.ProcessFile(file, async x =>
                {
                    var builtChunk = FileChunk.Build(file.Id, ++sequenceNo, x, ChunkSource.FromNewFile);
                    Log.Verbose($"Inserting chunk {builtChunk.SequenceNo} from '{file.Path}' into database");
                    await database.Execute(_insertStatement, new { builtChunk.Id, builtChunk.FileId, builtChunk.SequenceNo, builtChunk.Value, builtChunk.ChunkSource });
                });
            }

            return sequenceNo;
        }

        public async Task SplitByteArrayIntoChunks(byte[] bytes)
        {

        }
    }
}
