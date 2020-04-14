using System;

namespace Pi.Replicate.Domain
{
    public class FileChunk
    {
        public Guid Id { get; private set; }

        public Guid FileId { get; set; }

        public File File { get; private set; }

        public int SequenceNo { get; private set; }

        public byte[] Value { get; private set; }

        public ChunkSource ChunkSource { get; private set; }

        public static FileChunk Build(File file, int sequenceNo, byte[] value,ChunkSource chunkSource)
        {
            return new FileChunk
            {
                Id = Guid.NewGuid(),
                File = file,
                SequenceNo = sequenceNo,
                Value = value,
                ChunkSource = chunkSource
            };
        }
    }


    public class ChunkPackage
    {
        public Guid Id { get; private set; }

        public Guid FileChunkId { get; private set; }

        public FileChunk FileChunk { get; set; }

        public Guid RecipientId { get; private set; }

        public Recipient Recipient { get; set; }

        public static ChunkPackage Build(FileChunk fileChunk, Recipient recipient)
        {
            return new ChunkPackage
            {
                Id = Guid.NewGuid(),
                FileChunkId = fileChunk.Id,
                FileChunk = fileChunk,
                RecipientId = recipient.Id,
                Recipient = recipient
            };
        }
    }

    public enum ChunkSource
    {
        FromNewFile = 0,
        FromChangedFile = 1
    }

}