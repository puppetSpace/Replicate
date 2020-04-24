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
		public double SequenceNo { get; set; }

		public ReadOnlyMemory<byte> Value { get; set; }

		public ChunkSource ChunkSource { get; set; }
	}
}
