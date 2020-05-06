using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Domain
{
    public class SystemSetting
    {
        public Guid Id { get; private set; }

        public string Key { get; private set; }

        public string Value { get; private set; }

		public string DataType { get; private set; }

		public string Info { get; private set; }

	}
}
