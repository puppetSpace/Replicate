using System;

namespace Pi.Replicate.Domain
{
    public class FailedFile{
        public Guid Id { get; private set; }

        public File File { get; set; }

        public Recipient Recipient { get; set; }

        public static FailedFile Build(File file, Recipient recipient)
        {
            return new FailedFile { Id = Guid.NewGuid(), File = file, Recipient = recipient };
        }
    }
}