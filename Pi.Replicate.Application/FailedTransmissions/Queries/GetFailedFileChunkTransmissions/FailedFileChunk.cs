using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FailedTransmissions.Queries.GetFailedFileChunkTransmissions
{
    public class FailedFileChunk
    {
		public FileChunk FileChunk { get; set; }

		public Recipient Recipient { get; set; }
	}
}
