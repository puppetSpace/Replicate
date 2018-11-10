using System;

namespace Pi.Replicate.Schema
{
    public class FileChunk
    {
        public Guid Id { get; set; }

        public File File { get; set; }

        public int SequenceNo { get; set; }

        public string Value { get; set; }

        public ChunkStatus Status { get; set; }

        public override string ToString()
        {
            return $"File: {File?.Name}, SequenceNo: {SequenceNo}";
        }
    }


    //todo get failed chunks from database back on to the queue
    public class FailedUploadFileChunk
    {
        public Guid Id { get; set; }

        public FileChunk FileChunk { get; set; }

        public Host Host { get; set; }
    }

}