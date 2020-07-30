using System;
using System.Collections.Generic;

namespace Pi.Replicate.Worker.Host.Models
{
	public class Folder
	{
		public Folder()
		{

		}

		public Folder(string name, ICollection<Recipient> recipients)
		{
			Id = Guid.NewGuid();
			Name = name;
			Recipients = recipients;
		}

		public Guid Id { get; private set; }

		public string Name { get; private set; }

		public ICollection<Recipient> Recipients { get; set; }
	}
}
