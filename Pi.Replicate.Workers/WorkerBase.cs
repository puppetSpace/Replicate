using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pi.Replicate.Workers
{
    public abstract class WorkerBase
    {
        public abstract Thread DoWork(CancellationToken cancellationToken);
    }
}
