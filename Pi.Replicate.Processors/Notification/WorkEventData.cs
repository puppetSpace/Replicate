using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Notification
{
    public struct WorkEventData
    {
        public Type TypeOfWorkData { get; set; }

        public int CurrentWorkload { get; set; }

        public QueueKind QueueKind { get; set; }
    }
}
