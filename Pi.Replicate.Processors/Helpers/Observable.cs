using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Processors.Helpers
{
    public class Observable<TE> : IObservable<TE>
    {
        private List<IObserver<TE>> Observers { get; set; }

        public IDisposable Subscribe(IObserver<TE> observer)
        {
            return new Subscription(observer, Observers);
        }

        protected void Notify(TE value)
        {
            foreach(var observer in Observers)
            {
                observer.OnNext(value);
            }
        }

        protected void NotifyComplete()
        {
            foreach (var observer in Observers)
            {
                observer.OnCompleted();
            }
        }

        private class Subscription : IDisposable
        {
            private readonly IObserver<TE> _observer;
            private readonly IList<IObserver<TE>> _observers;

            public Subscription(IObserver<TE> observer, IList<IObserver<TE>> observers)
            {
                _observer = observer;
                _observers = observers;

                if (_observer != null && !_observers.Contains(_observer))
                    _observers.Add(_observer);
            }

            public void Dispose()
            {
                if (_observer != null && _observers.Contains(_observer))
                    _observers.Remove(_observer);
            }
        }
    }
}
