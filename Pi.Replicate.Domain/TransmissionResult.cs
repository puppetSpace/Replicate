using System;

namespace Pi.Replicate.Domain
{
    public class TransmissionResult
    {
        public Recipient Recipient { get; set; }
        public FileChunk FileChunk { get; set; }
		public DateTime CreationTime { get; set; }
    }
}