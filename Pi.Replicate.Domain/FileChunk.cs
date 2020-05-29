using System;

namespace Pi.Replicate.Domain
{
    public class FileChunk
    {
        public Guid Id { get; private set; }

        public Guid FileId { get; private set; }

        public int SequenceNo { get; private set; }

        public ReadOnlyMemory<byte> Value { get; private set; }

        public static FileChunk Build(Guid fileId, int sequenceNo, ReadOnlyMemory<byte> value)
        {
            return new FileChunk
            {
                Id = Guid.NewGuid(),
                FileId = fileId,
                SequenceNo = sequenceNo,
                Value = value,
            };
        }
    }
}