namespace Pi.Replicate.Domain
{
    public class TransmissionResult
    {
        public Recipient Recipient { get; set; }
        public FileChunk FileChunk { get; set; }
    }
}