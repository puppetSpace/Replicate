using Pi.Replicate.Shared;
using Pi.Replicate.Shared.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pi.Replicate.Worker.Host.Services
{
	public class SystemService
	{
		public async Task<SystemOverview> GetSystemOverview()
		{
			var overview = new SystemOverview();
			using (var process = Process.GetCurrentProcess())
			{
				overview.MemoryUsage = ByteDisplayConverter.Convert(process.PrivateMemorySize64);
				var gcMemoryInfo = GC.GetGCMemoryInfo();
				overview.MemoryUsagePercentage = (process.PrivateMemorySize64 / gcMemoryInfo.TotalAvailableMemoryBytes) * 100;
				overview.CpuUsagePercentage = await GetCpuUsage(process);
			}

			return overview;
		}

		private async Task<double> GetCpuUsage(Process process)
		{
			var startTime = DateTime.UtcNow;
			var startCpuUsage = process.TotalProcessorTime;
			await Task.Delay(500);
			var endTime = DateTime.UtcNow;
			var endCpuUsage = process.TotalProcessorTime;

			var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
			var totalMsPassed = (endTime - startTime).TotalMilliseconds;

			var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

			return cpuUsageTotal * 100;
		}
	}
}
