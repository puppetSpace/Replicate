using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Notification
{
    public class WorkEventData
    {
        public QueueKind QueueKind { get; set; }

        public Type TypeOfData { get; set; }

        public int CurrentWorkload { get; set; }
    }
}
