﻿using Microsoft.Extensions.Configuration;
using Pi.Replicate.Processors.Helpers;
using Pi.Replicate.Processors.Repositories;
using Pi.Replicate.Schema;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;


namespace Pi.Replicate.Processors.Files
{
    internal class FileSplitter : Worker<File, FileChunk>
    {
        private readonly uint _sizeofChunkInBytes;
        private readonly IRepository _repository;

        public FileSplitter(IConfiguration configuration, IWorkItemQueueFactory workItemQueueFactory, IRepository repository)
            :base(workItemQueueFactory, QueueKind.Outgoing)
        {
            _sizeofChunkInBytes = uint.Parse(configuration[Constants.FileSplitSizeOfChunksInBytes]);
            _repository = repository;
        }

        protected override async Task DoWork(File file)
        {
            var path = file?.GetPath();
            if (!String.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path) && !FileLock.IsLocked(path))
            {
                using (var stream = System.IO.File.OpenRead(path))
                {
                    await SplitStream(file, stream);
                }
            }
        }

        private async Task SplitStream(File file, System.IO.Stream stream)
        {
            var buffer = new byte[_sizeofChunkInBytes];
            int bytesRead = 0;
            int chunksCreated = 0;
            MD5 hashCreator = MD5.Create();
            //todo compression
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                hashCreator.TransformBlock(buffer, 0, bytesRead, null, 0);

                var toWriteBytes = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, toWriteBytes, 0, bytesRead);

                await AddItem(new FileChunk
                {
                    Id = Guid.NewGuid(),
                    File = file,
                    SequenceNo = ++chunksCreated,
                    Value = Convert.ToBase64String(toWriteBytes),
                    Status = ChunkStatus.New
                });
            }

            hashCreator.TransformFinalBlock(buffer, 0, bytesRead);
            file.Hash = Convert.ToBase64String(hashCreator.Hash);
            file.AmountOfChunks = chunksCreated;
            file.Status = FileStatus.Sent;
            await _repository.FileRepository.Update(file);
        }

    }
}