using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedEofMessageTransmissions
{
    public class FailedEofMessage
    {
		public EofMessage EofMessage { get; set; }

		public Recipient Recipient { get; set; }
	}
}
