using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.FileChunks.Queries.GetFailedTransmissions
{
    public class FailedFileChunk
    {
		public FileChunk FileChunk { get; set; }

		public Recipient Recipient { get; set; }
	}
}
