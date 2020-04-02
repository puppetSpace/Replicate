using System;

namespace Pi.Replicate.Domain
{
    public class Recipient
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }
    }
}