using Pi.Replicate.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Builders
{
    internal static class FileChunkBuilder
    {

        internal static FileChunk Build(File file, int sequenceNo, byte[] value)
        {
            return new FileChunk
            {
                Id = Guid.NewGuid(),
                File = file,
                SequenceNo = sequenceNo,
                Value = Convert.ToBase64String(value),
                Status = ChunkStatus.New
            };
        }
    }
}
