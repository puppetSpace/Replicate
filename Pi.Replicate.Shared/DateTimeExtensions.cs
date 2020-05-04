using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared
{
    public static class DateTimeExtensions
    {
        public static DateTime TruncateMilliseconds(this DateTime dateTime)
		{
			return new DateTime(dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond), dateTime.Kind);
		}
    }
}
