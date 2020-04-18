using System;

namespace Pi.Replicate.Domain
{
    public class FileChunk
    {
        public Guid Id { get; private set; }

        public Guid FileId { get; private set; }

        public double SequenceNo { get; private set; }

        public byte[] Value { get; private set; }

        public ChunkSource ChunkSource { get; private set; }

        public static FileChunk Build(Guid fileId, double sequenceNo, byte[] value,ChunkSource chunkSource)
        {
            return new FileChunk
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                SequenceNo = sequenceNo,
                Value = value,
                ChunkSource = chunkSource
            };
        }
    }


    public class ChunkPackage
    {
        public FileChunk FileChunk { get; set; }

        public Recipient Recipient { get; set; }

        public static ChunkPackage Build(FileChunk fileChunk, Recipient recipient)
        {
            return new ChunkPackage
            {
                FileChunk = fileChunk,
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