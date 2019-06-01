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


    public class HostFileChunk : FileChunk
    {
		public HostFileChunk()
		{

		}

		public HostFileChunk(FileChunk fileChunk)
		{
			Id = fileChunk.Id;
			File = fileChunk.File;
			SequenceNo = fileChunk.SequenceNo;
			Value = fileChunk.Value;
			Status = fileChunk.Status;
		}

        public Host Host { get; set; }
    }

}