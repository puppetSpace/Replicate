using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common.Interfaces;
using Pi.Replicate.Application.Files.Processing;
using Pi.Replicate.Domain;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Services
{
    public class ChunkService
    {
        private readonly FileSplitterFactory _fileSplitterFactory;
        private readonly IDatabaseFactory _databaseFactory;
        private readonly IConfiguration _configuration;
        private const string _insertStatement = "INSERT INTO dbo.FileChunk(Id,FileId,SequenceNo,Value,ChunkSource) VALUES (@Id,@FileId,@SequenceNo,@Value,@ChunkSource)";


        public ChunkService(FileSplitterFactory fileSplitterFactory, IDatabaseFactory databaseFactory, IConfiguration configuration)
        {
            _fileSplitterFactory = fileSplitterFactory;
            _databaseFactory = databaseFactory;
            _configuration = configuration;
        }

        public async Task<int> SplitFileIntoChunks(File file)
        {
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
                    await database.Execute(_insertStatement, new { builtChunk.Id, builtChunk.FileId, builtChunk.SequenceNo, Value = builtChunk.Value.ToArray(), builtChunk.ChunkSource });
                });
            }

            return sequenceNo;
        }

        public async Task<int> SplitMemoryIntoChunks(File forFile,int startofSequenceNo,ReadOnlyMemory<byte> memory)
        {
            var sizeofChunkInBytes = int.Parse(_configuration[Constants.FileSplitSizeOfChunksInBytes]);
            sizeofChunkInBytes = memory.Length>sizeofChunkInBytes ? sizeofChunkInBytes : memory.Length;
            var database = _databaseFactory.Get();
            var indexOfSlice = 0;
            double sequenceNo = startofSequenceNo;
            var amountOfChunks = 0;
            using (database)
            {
                database.Connection.Open();
                while (indexOfSlice < memory.Length)
                {
                    sequenceNo = sequenceNo + 0.0001;
                    var builtChunk = FileChunk.Build(forFile.Id, sequenceNo, memory.Slice(indexOfSlice,sizeofChunkInBytes), ChunkSource.FromChangedFile);
                    Log.Verbose($"Inserting delta chunk {builtChunk.SequenceNo} from changed '{forFile.Path}' into database");
                    await database.Execute(_insertStatement, new { builtChunk.Id, builtChunk.FileId, builtChunk.SequenceNo, Value = builtChunk.Value.ToArray(), builtChunk.ChunkSource });
                    indexOfSlice += sizeofChunkInBytes;
                    amountOfChunks++;
                }
            }

            return amountOfChunks;
        }
    }
}
