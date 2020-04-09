using Microsoft.Extensions.Configuration;
using Pi.Replicate.Application.Common;
using Pi.Replicate.Domain;
using Pi.Replicate.Processing.Helpers;
using Pi.Replicate.Shared;
using Serilog;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace Pi.Replicate.Processing.Files
{
    public sealed class FileSplitter
    {
        private readonly int _sizeofChunkInBytes;
        private readonly PathBuilder _pathBuilder;
        private static (List<byte[]> Chunks, byte[] Hash) _emptyResultResult = (new List<byte[]>(),new byte[0]);

        public FileSplitter(IConfiguration configuration, PathBuilder pathBuilder)
        {
            _sizeofChunkInBytes = int.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
            _pathBuilder = pathBuilder;
        }

        public async Task<(List<byte[]> Chunks, byte[] Hash)> ProcessFile(File file)
        {
            var path = _pathBuilder.BuildPath(file);
            
            if (!String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
            {
                Log.Verbose($"File '{path}' is being processed and turned into chunks of {_sizeofChunkInBytes} bytes");
                using (var stream = System.IO.File.OpenRead(path))
                {
                    return await SplitStream(file, stream);
                }
            }
            else
            {
                Log.Warning($"File '{path}' does not exist or is locked. File will not be processedd");
                return _emptyResultResult;
            }
        }

        private async Task<(List<byte[]> Chunks,byte[] Hash)> SplitStream(File file, System.IO.Stream stream)
        {
            var chunks = new List<byte[]>();
            var buffer = ArrayPool<byte>.Shared.Rent(_sizeofChunkInBytes);
            MD5 hashCreator = MD5.Create();
            int bytesRead;
            //todo compression
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);
                chunks.Add(buffer);

            }
            hashCreator.TransformFinalBlock(buffer, 0, bytesRead);

            ArrayPool<byte>.Shared.Return(buffer);
            return (chunks,hashCreator.Hash);
        }

    }


    public class FileSplitterFactory
    {
        private readonly IConfiguration _configuration;
        private readonly PathBuilder _pathBuilder;

        public FileSplitterFactory(IConfiguration configuration, PathBuilder pathBuilder)
        {
            _configuration = configuration;
            _pathBuilder = pathBuilder;
        }

        public FileSplitter Get()
        {
            return new FileSplitter(_configuration, _pathBuilder);
        }
    }
}
