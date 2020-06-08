using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host
{
    public static class WorkerLog
    {
		public static ILogger Instance { get; } = Log.ForContext("Context", "Host");
    }
}
