using System;

namespace Pi.Replicate.Domain
{
    public class FileChunk
    {
        public Guid Id { get; private set; }

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

        public Guid RecipientId { get; private set; }

        public static ChunkPackage Build(Guid fileChunkId, Guid recipientId)
        {
            return new ChunkPackage
            {
                Id = Guid.NewGuid(),
                FileChunkId = fileChunkId,
                RecipientId = recipientId
            };
        }
    }


    public enum ChunkSource
    {
        FromNewFile = 0,
        FromChangedFile = 1
    }

}