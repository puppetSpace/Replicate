using System;

namespace Pi.Replicate.Domain
{
    public class Recipient
    {
        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string Address { get; private set; }

		public bool Confirmed { get; private set; }

		public void ConfirmExistence(){
			Confirmed = true;
		}

        public static Recipient Build(string name, string address)
        {
            return new Recipient { Id = Guid.NewGuid(), Name = name, Address = address };
        }
    }
}