using Pi.Replicate.Processors.Builders;
using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace Pi.Replicate.Processors
{
    internal class FileSplitter : Observable<FileChunk>
    {
        private readonly File _file;
        private readonly uint _sizeofChunkInBytes;

        public FileSplitter(File file, uint sizeofChunkInBytes)
        {
            _file = file;
            _sizeofChunkInBytes = sizeofChunkInBytes;
        }

        public async Task<File> Split()
        {
            if (_file != null && System.IO.File.Exists(_file.GetPath()))
            {
                using (var stream = System.IO.File.OpenRead(_file.GetPath()))
                {
                    await SplitStream(stream);
                }
            }

            return _file;
        }

        private async Task SplitStream(System.IO.Stream stream)
        {
            var buffer = new byte[_sizeofChunkInBytes];
            int bytesRead = 0;
            int chunksCreated = 0;
            MD5 hashCreator = MD5.Create();
            //todo compression
            while ((bytesRead = await stream.ReadAsync(buffer,0,buffer.Length)) > 0)
            {
                hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);

                var toWriteBytes = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, toWriteBytes, 0, bytesRead);
                
                Notify(FileChunkBuilder.Build(_file,++chunksCreated, toWriteBytes));
            }

            NotifyComplete();

            hashCreator.TransformFinalBlock(buffer, 0, bytesRead);
            _file.Hash = Convert.ToBase64String(hashCreator.Hash);
            _file.AmountOfChunks = chunksCreated;
        }

    }
}
