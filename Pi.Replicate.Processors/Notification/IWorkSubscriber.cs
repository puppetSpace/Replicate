using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Notification
{
    public interface IWorkSubscriber
    {
        void WorkCreated(WorkEventData workEventData);
    }
}
