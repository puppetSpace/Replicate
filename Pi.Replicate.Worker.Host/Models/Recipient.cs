using System;

namespace Pi.Replicate.Worker.Host.Models
{
	public class Recipient
	{
		public Recipient()
		{

		}

		public Recipient(string name, string address, bool verified)
		{
			Id = Guid.NewGuid();
			Name = name;
			Address = address;
			Verified = verified;
		}

		public Guid Id { get; private set; }

		public string Name { get; private set; }

		public string Address { get; private set; }

		public bool Verified { get; private set; }
	}
}