using Pi.Replicate.Processors.Builders;
using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors
{
    public partial class FileSplitter : Observable<FileChunk>
    {
        private readonly List<IObserver<FileChunk>> _observers = new List<IObserver<FileChunk>>();
        private readonly File _file;

        public FileSplitter(File file)
        {
            _file = file;
        }

        public async Task<File> Split()
        {
            using(var stream = System.IO.File.OpenRead(_file.GetPath()))
            {
                await SplitStream(stream);
            }

            return _file;
        }

        private async Task SplitStream(System.IO.Stream stream)
        {
            var buffer = new byte[1024*512]; //512kb TODO make config of this
            int bytesRead = 0;
            int chunksCreated = 0;
            MD5 hashCreator = MD5.Create();
            while ((bytesRead = await stream.ReadAsync(buffer,0,buffer.Length)) > 0)
            {
                chunksCreated++;
                hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);
                Notify(FileChunkBuilder.Build(_file,chunksCreated, buffer));
            }
            NotifyComplete();
            hashCreator.TransformFinalBlock(buffer, 0, bytesRead);
            _file.Hash = Convert.ToBase64String(hashCreator.Hash);
            _file.AmountOfChunks = chunksCreated;
        }

    }
}
