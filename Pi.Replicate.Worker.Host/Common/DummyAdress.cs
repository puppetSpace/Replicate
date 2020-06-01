using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Common
{
    public class DummyAdress
    {
		public static string Create(string host) => $"https://{host}";
    }
}
