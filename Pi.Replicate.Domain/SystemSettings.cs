using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class SystemSetting
    {
        public Guid Id { get; set; }

        public string Key { get; set; }

        public string Value { get; set; }
    }
}
