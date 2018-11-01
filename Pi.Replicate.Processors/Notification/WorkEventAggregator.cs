using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processing.Notification
{
    public class WorkEventAggregator : IWorkEventAggregator
    {
        private readonly object _locker = new object();
        private readonly List<IWorkSubscriber> _subscribers = new List<IWorkSubscriber>();

        public void Publish(WorkEventData workEventData)
        {
            lock (_locker)
            {
                foreach (var subscriber in _subscribers)
                    subscriber.WorkCreated(workEventData);
            }
        }

        public void Subscribe(IWorkSubscriber workSubscriber)
        {
            lock (_locker)
                _subscribers.Add(workSubscriber);
        }
    }
}
