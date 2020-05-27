using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.Models
{
    public class LogMessage
    {
		public LogMessage()
		{

		}
		public LogMessage(string content, LogEventLevel level)
		{
			Content = content;
			Level = level;
		}

		public string Content { get; set; }

		public LogEventLevel Level { get; set; }
	}
}
