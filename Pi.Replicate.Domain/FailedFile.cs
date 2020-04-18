using System;

namespace Pi.Replicate.Domain
{
    public class FailedFile{
        public File File { get; set; }

        public Recipient Recipient { get; set; }

        public static FailedFile Build(File file, Recipient recipient)
        {
            return new FailedFile { File = file, Recipient = recipient };
        }
    }
}