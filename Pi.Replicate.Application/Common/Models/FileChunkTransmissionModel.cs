using Pi.Replicate.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Application.Common.Models
{
    public class FileChunkTransmissionModel
    {
		public int SequenceNo { get; set; }

		public byte[] Value { get; set; }

		public string Host { get; set; }
	}
}
