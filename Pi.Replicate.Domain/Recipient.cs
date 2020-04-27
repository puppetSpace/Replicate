using System;

namespace Pi.Replicate.Domain
{
    public class Recipient
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string Address { get; private set; }

		public bool Verified { get; private set; }

		public void IsVerified(){
			Verified = true;
		}

        public static Recipient Build(string name, string address, bool verified)
        {
            return new Recipient { Id = Guid.NewGuid(), Name = name, Address = address, Verified = verified };
        }
    }
}