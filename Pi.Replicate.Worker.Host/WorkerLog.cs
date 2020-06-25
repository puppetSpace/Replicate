using Serilog;

namespace Pi.Replicate.Worker.Host
{
	public static class WorkerLog
	{
		public static ILogger Instance { get; } = Log.ForContext("Context", "Host");
	}
}
