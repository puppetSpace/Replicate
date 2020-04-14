using System;

namespace Pi.Replicate.Domain
{
    public class FailedFile{
        public Guid Id { get; private set; }

        public Guid FileId { get; private set; }

        public File File { get; set; }

        public Guid RecipientId { get; private set; }

        public Recipient Recipient { get; set; }

        public static FailedFile Build(Guid fileId, Guid recipientId)
        {
            return new FailedFile { Id = Guid.NewGuid(), FileId = fileId, RecipientId = recipientId };
        }
    }
}