using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Queues
{
    public static class WorkLoadQueueCache
    {
        private static Dictionary<Type, ICollection> _queues = new Dictionary<Type, ICollection>();
        private static object _object = new object();

        public static BlockingCollection<TE> CreateIfNotExist<TE>()
        {
            lock (_object)
            {
                if (!_queues.ContainsKey(typeof(TE)))
                    _queues.Add(typeof(TE), new BlockingCollection<TE>());

                return Get<TE>();
            }
        }

        public static BlockingCollection<TE> Get<TE>()
        {
            lock (_object)
            {
                if (_queues.ContainsKey(typeof(TE)))
                    return _queues[typeof(TE)] as BlockingCollection<TE>;
                else
                    throw new InvalidOperationException($"No queue exits yet for type {typeof(TE).Name}. Please create one first");
            }
        }
    }
}
