using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public enum ChunkStatus
    {
        New = 0,
        Error = 1,
        Uploading = 2,
        Received = 3
    }
}
