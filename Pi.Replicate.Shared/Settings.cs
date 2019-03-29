using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public class Settings : ISettings
    {
        private readonly IDictionary<string, object> _internalSettings = new Dictionary<string, object>();

        public object this[string key]
        {
            get
            {
                if (String.IsNullOrEmpty(key))
                    throw new ArgumentNullException(nameof(key));

                var lcKey = key.ToLowerInvariant();
                if (!_internalSettings.ContainsKey(lcKey))
                    throw new KeyNotFoundException($"Key {lcKey} not found in settings");

                return _internalSettings[lcKey];
            }
            set
            {
                if (String.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                var lcKey = key.ToLowerInvariant();
                _internalSettings[lcKey] = value;
            }
        }
    }
}
