using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class FailedTransmission
    {
		public Guid Id { get; private set; }

		public Guid? FileId { get; private set; }

		public Guid? EofMessageId { get; private set; }

		public Guid? FileChunkId { get; private set; }

		public Guid RecipientId { get; private set; }

		public DateTime CreationTime { get; private set; }
	}
}
