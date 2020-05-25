using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pi.Replicate.Shared.Models
{
    public class SystemOverview
    {
		public string MemoryUsage { get; set; }
		public double MemoryUsagePercentage { get; set; }
		public double CpuUsagePercentage { get; set; }
	}
}
