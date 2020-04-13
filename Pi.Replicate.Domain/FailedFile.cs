using System;

namespace Pi.Replicate.Domain
{
    public class FailedFile{
        public Guid Id { get; set; }

        public Guid FileId { get; set; }

        public Guid RecipientId { get; set; }
    }
}