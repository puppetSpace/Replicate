using System;

namespace Pi.Replicate.Worker.Host.Models
{
    public class FileChunk
    {
        public Guid Id { get; private set; }

        public Guid FileId { get; private set; }

        public int SequenceNo { get; private set; }

        public byte[] Value { get; private set; }


        public static FileChunk Build(Guid fileId, int sequenceNo, byte[] value)
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