using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Models
{
    public class Folder
    {
		public Guid Id { get; set; }

		public string Name { get; set; }

		public ICollection<Recipient> Recipients { get; set; }
	}
}
