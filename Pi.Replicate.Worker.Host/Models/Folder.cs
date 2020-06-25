using System;
using System.Collections.Generic;

namespace Pi.Replicate.Worker.Host.Models
{
	public class Folder
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public ICollection<Recipient> Recipients { get; set; }
	}
}
