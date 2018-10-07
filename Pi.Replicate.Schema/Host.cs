using System;

namespace Pi.Replicate.Schema
{
    public class Host
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Uri Address { get; set; }
    }
}