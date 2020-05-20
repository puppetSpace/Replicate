using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Notifications.Models
{
    public class RecipientsAddedNotification
    {
		public Guid FolderId { get; set; }

		public List<Recipient> Recipients { get; set; }
	}
}
