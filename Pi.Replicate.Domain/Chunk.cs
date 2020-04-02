using System;

namespace Pi.Replicate.Domain
{
    public class FileChunk
    {
        public Guid Id { get; set; }

        public File File { get; set; }

        public int SequenceNo { get; set; }

        public string Value { get; set; }

        public ChunkStatus Status { get; set; }
    }


    public class ChunkPackage
    {
        public Guid Id { get; set; }

        public FileChunk FileChunk { get; set; }

        public Recipient Recipient { get; set; }

        public ChunkPackageType Type { get; set; }
    }

    public enum ChunkPackageType
    {
        None = 0,
        Failed = 1,
        Requested = 2,
        ToRequest = 3
    }

}