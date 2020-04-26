using Pi.Replicate.Domain;

namespace Pi.Replicate.Application.Files.Queries.GetFailedTransmissions
{
    public class FailedFile
    {
		public Folder Folder { get; set; }
		public Recipient Recipient { get; set; }
		public File File { get; set; }
    }
}